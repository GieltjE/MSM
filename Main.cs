// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2021 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;
using MSM.Extends.Themes;
using MSM.Functions;
using MSM.Service;
using MSM.UIElements;
using WeifenLuo.WinFormsUI.Docking;
using Settings = MSM.Service.Settings;

namespace MSM
{
    public partial class Main : FormOptimized
    {
        public readonly NotifyIcon NotifyIcon = new();
        private readonly ContextMenuStrip _contextMenuStripTrayIcon = new();
        private readonly ToolStripMenuItem _trayIconStripOpen = new();
        private readonly ToolStripMenuItem _trayIconStripExit = new();
        private FormWindowState _previousWindowState = FormWindowState.Normal;
        private readonly Dictionary<String, (UserControlOptimized userControlOptimized, DockState dockState, ToolStripMenuItem toolStripMenuItem, DockContentOptimized dockContentOptimized)> _defaultUserControls = new(StringComparer.Ordinal);
        private Boolean LoadingTab { get; set; }
        private readonly VisualStudioToolStripExtender _visualStudioToolStripExtender = new();
        private readonly Dictionary<TerminalControl, DockContentOptimized> _terminalControls = new();
        private readonly LogControl _logControl;
        private readonly SemaphoreSlim _saveSessionsSemaphoreSlim = new(1, 1);

        public Main()
        {
            String[] commandLineArguements = Environment.GetCommandLineArgs();
            if (commandLineArguements.Contains("--update", StringComparer.Ordinal))
            {
                String parentDirectory = Path.Combine(FileOperations.GetRunningDirectory(), "..");
                List<String> files = FileOperations.ReturnFiles(parentDirectory, false, includedExtensions: new[] { ".dll", ".exe", ".pdb", ".xml", ".config", ".md" });
                foreach (String file in files)
                {
                    while (!FileOperations.DeleteFile(file))
                    {
                        Thread.Sleep(200);
                    }
                }

                files = FileOperations.ReturnFiles(FileOperations.GetRunningDirectory(), false);
                foreach (String file in files)
                {
                    while (!FileOperations.CopyFile(file, Path.Combine(parentDirectory, Path.GetFileName(file)), true))
                    {
                        Thread.Sleep(200);
                    }
                }

                ProcessStartInfo procInfo = new(Path.Combine(parentDirectory, Path.GetFileName(FileOperations.GetCurrentExecutable())))
                {
                    WorkingDirectory = parentDirectory,
                    Arguments = "--updated",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Maximized,
                    UseShellExecute = false
                };
                Process.Start(procInfo);
                Environment.Exit(1002);
            }
            else if (commandLineArguements.Contains("--updated"))
            {
                String updateDirectory = Path.Combine(FileOperations.GetRunningDirectory(), "update");
                while (FileOperations.DeleteDirectory(updateDirectory, true).Any())
                {
                    Thread.Sleep(5000);
                }
            }

            InitializeComponent();
            Variables.MainForm = this;
            Variables.MainSTAThreadID = Thread.CurrentThread.ManagedThreadId;

            FileOperations.CreateDirectory(Variables.SettingsDirectory);
            if (Settings.Values.CheckForUpdates)
            {
                Settings.Values.CheckForUpdates = true;
            }
            if (Settings.Values.AlwaysShowTrayIcon)
            {
                Settings.Values.AlwaysShowTrayIcon = true;
            }
            if (Settings.Values.ForceCloseSessionsOnCrash)
            {
                Settings.Values.ForceCloseSessionsOnCrash = true;
            }

            Variables.MainForm.DockPanel_Main.Theme = Settings.Values.Theme switch
            {
                Enumerations.Theme.Blue => new VS2015BlueTheme(),
                Enumerations.Theme.Black => new MaterialDarkTheme(),
                Enumerations.Theme.Dark => new VS2015DarkTheme(),
                Enumerations.Theme.Light => new VS2015LightTheme(),
                _ => throw new ArgumentOutOfRangeException()
            };
            Variables.ColorPalette = DockPanel_Main.Theme.ColorPalette;
            Variables.Measures = DockPanel_Main.Theme.Measures;

            _visualStudioToolStripExtender.SetStyle(ToolStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel_Main.Theme);
            _visualStudioToolStripExtender.SetStyle(StatusStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel_Main.Theme);

            Service.Events.ShutDownFired += ShutDownFired;
            Service.Events.ProcessExited += OnServerExited;
            
            if (Settings.Values.MaximizeOnStart)
            {
                WindowState = FormWindowState.Maximized;
            }

            _logControl = new LogControl();
            _defaultUserControls.Add("Logs", (_logControl, DockState.DockBottomAutoHide, ToolStrip_ShowLogs, null));
            _defaultUserControls.Add("Settings", (new SettingControl(), DockState.Document, ToolStrip_ShowSettings, null));
            _defaultUserControls.Add("Serverlist", (new ServerControl(), DockState.DockRight, ToolStrip_ShowServerList, null));

            Logger.LogAdded += _logControl.LoggerOnLogAdded;

            NotifyIcon.Icon = new Icon(Icon, 16, 16);
            NotifyIcon.ContextMenuStrip = _contextMenuStripTrayIcon;
            NotifyIcon.MouseClick += TrayIconMouseClick;
            NotifyIcon.DoubleClick += TrayIconDoubleClick;
            NotifyIcon.BalloonTipClicked += TrayIconStripOpenClick;

            _trayIconStripOpen.Text = @"Open";
            _trayIconStripOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _trayIconStripOpen.Click += TrayIconStripOpenClick;

            _trayIconStripExit.Text = @"Close";
            _trayIconStripExit.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _trayIconStripExit.Click += TrayIconStripExitClick;

            _contextMenuStripTrayIcon.Items.AddRange(new ToolStripItem[] { _trayIconStripOpen, _trayIconStripExit });

            NotifyIcon.Visible = Settings.Values.AlwaysShowTrayIcon;
        }
        private void MainShown(Object sender, EventArgs e)
        {
            Boolean loadSuccess = false;
            if (File.Exists(Variables.SessionFile))
            {
                try
                {
                    DockPanel_Main.LoadFromXml(Variables.SessionFile, GetContentFromPersistString);
                    
                    foreach (DockPane dockPane in DockPanel_Main.Panes)
                    {
                        IDockContent previouslyActiveContent = dockPane.ActiveContent;
                        foreach (IDockContent content in dockPane.Contents.Reverse().Where(x => !x.DockHandler.IsHidden && x.DockHandler.VisibleState != DockState.Hidden))
                        {
                            if (content == previouslyActiveContent) continue;
                            if (_defaultUserControls.ContainsKey(((DockContentOptimized)content).Name)) continue;
                            
                            dockPane.ActiveContent = content;
                        }
                        dockPane.ActiveContent = previouslyActiveContent;
                    }
                    loadSuccess = true;
                }
                catch (Exception exception)
                {
                    Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not reinstate (all) tabbages", exception);
                }
            }
            if (!loadSuccess)
            {
                ToolStripButtonDefaultControlClick(ToolStrip_ShowServerList, null);
                ToolStripButtonDefaultControlClick(ToolStrip_ShowLogs, null);
            }

            foreach (KeyValuePair<String, (UserControlOptimized _, DockState _d, ToolStripMenuItem toolStripMenuItem, DockContentOptimized _c)> dockContentOptimized in _defaultUserControls.Where(x => x.Value.dockContentOptimized is { IsHidden: false }))
            {
                dockContentOptimized.Value.toolStripMenuItem.Checked = true;
            }

            if (Settings.Values.InitialSessions != Enumerations.InitialSessions.Predefined)
            {
                Variables.StartupComplete = true;
                return;
            }

            IOrderedEnumerable<Server> orderedList = Settings.AllServers.Values.Where(allServersValue => allServersValue.PredefinedStartIndex != -1).OrderBy(x => x.PredefinedStartIndex);
            foreach (Server server in orderedList)
            {
                AddServer(server, false);
            }

            Variables.StartupComplete = true;
        }
        private IDockContent GetContentFromPersistString(String persistString)
        {
            Boolean add = false;
            Control content;
            String displayName, internalName;
            if (_defaultUserControls.ContainsKey(persistString))
            {
                (UserControlOptimized userControlOptimized, DockState _, ToolStripMenuItem _, DockContentOptimized _) = _defaultUserControls[persistString];
                content = userControlOptimized;
                internalName = persistString;
                displayName = persistString;
            }
            else
            {
                if (Settings.Values.InitialSessions != Enumerations.InitialSessions.Previous) return null;

                Server server = Settings.FindServer(persistString);
                if (server == null) return null;

                add = true;
                content = new TerminalControl(server);
                internalName = server.NodeID;
                displayName = server.DisplayName;
            }

            DockContentOptimized dockContent = AddDockContent(displayName, internalName, content, DockState.Float, false, false);
            if (!add) return dockContent;

            lock (_terminalControls)
            {
                _terminalControls.Add((TerminalControl)content, dockContent);
            }
            return dockContent;
        }
        private void MainClosing(Object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;

            switch (Settings.Values.CloseAction)
            {
                case Enumerations.CloseAction.Minimize:
                    WindowState = FormWindowState.Minimized;
                    break;
                case Enumerations.CloseAction.MinimizeToTray:
                    WindowState = FormWindowState.Minimized;
                    ShowInTaskbar = false;
                    NotifyIcon.Visible = true;
                    break;
                default:
                    Service.Events.ShutDown();
                    break;
            }
            e.Cancel = true;
        }
        private void MainResize(Object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                _previousWindowState = WindowState;
            }

            if (WindowState == FormWindowState.Minimized && Settings.Values.MinimizeToTray)
            {
                ShowInTaskbar = false;
                NotifyIcon.Visible = true;
            }
        }
        private void ShutDownFired()
        {
            Logger.LogAdded -= _logControl.LoggerOnLogAdded;
            Service.Events.ShutDownFired -= ShutDownFired;
            Service.Events.ProcessExited -= OnServerExited;
            NotifyIcon.Visible = false;
        }

        public void TrayIconDoubleClick(Object sender, EventArgs e)
        {
            if (e is not MouseEventArgs { Button: MouseButtons.Left }) return;

            TrayIconStripOpenClick(null, null);
        }
        private void TrayIconMouseClick(Object sender, MouseEventArgs e)
        {
            if (e is not {Button: MouseButtons.Right}) return;

            NotifyIcon.ContextMenuStrip.Show();
        }
        private static void TrayIconStripExitClick(Object sender, EventArgs e)
        {
            Service.Events.ShutDown();
        }
        private void TrayIconStripOpenClick(Object sender, EventArgs e)
        {
            if (!Settings.Values.AlwaysShowTrayIcon)
            {
                NotifyIcon.Visible = false;
            }

            ShowInTaskbar = true;
            WindowState = _previousWindowState;
            Show();
            BringToFront();
        }

        private void ToolStripButtonDefaultControlClick(Object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            String tag = (String)item.Tag;

            if (item.Checked)
            {
                item.Checked = false;
                _defaultUserControls[tag].dockContentOptimized.Hide();
                SaveSessions();
                return;
            }

            item.Checked = true;
            (UserControlOptimized userControlOptimized, DockState dockState, ToolStripMenuItem _, DockContentOptimized dockContentOptimized) = _defaultUserControls[tag];
            if (dockContentOptimized != null)
            {
                dockContentOptimized.Show();
            }
            else
            {
                AddDockContent(tag, tag, userControlOptimized, dockState);
            }
            SaveSessions();
        }
        private void ToolStripExitClick(Object sender, EventArgs e)
        {
            Service.Events.ShutDown();
        }
        private void ToolStripMenuItemGitHubClick(Object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/GieltjE/MSM/");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch {}
        }
        private void ToolStripMenuItemAboutClick(Object sender, EventArgs e)
        {
            new About(this).Show();
        }
        private void ToolStripMenuItemCheckForUpdateClick(Object sender, EventArgs e)
        {
            UpdateCheck.TriggerUpdateCheckJob();
        }
        private void ToolStripCommandKeyUp(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || !Settings.Values.SendCommandOnEnter) return;
            if (ToolStrip_Command.TextBox == null) return;

            SendCommand(ToolStrip_Command.TextBox.Text);

            if (Settings.Values.ClearCommandAfterSend)
            {
                ToolStrip_Command.TextBox.Text = "";
            }
        }
        private void ToolStripSendClick(Object sender, EventArgs e)
        {
            if (ToolStrip_Command.TextBox == null) return;

            SendCommand(ToolStrip_Command.TextBox.Text);
            if (Settings.Values.ClearCommandAfterSend)
            {
                ToolStrip_Command.TextBox.Text = "";
            }
        }

        private void OnServerExited(TerminalControl terminal)
        {
            if (!Settings.Values.CloseTabOnCrash) return;
            if (InvokeRequired)
            {
                Invoke(new Action<TerminalControl>(OnServerExited), terminal);
                return;
            }

            lock (_terminalControls)
            {
                if (!_terminalControls.ContainsKey(terminal)) return;

                _terminalControls[terminal].Close();
                _terminalControls.Remove(terminal);
            }
        }
        public void AddServer(Server server, Boolean save)
        {
            if (server == null) return;

            ThreadHelpers thread = new();
            thread.ExecuteThreadParameter(AddServerHelper, (server, save), false, false);
        }
        private void AddServerHelper(Object startInfo)
        {
            (Server server, Boolean save) = ((Server server, Boolean save))startInfo;

            TerminalControl terminal = new(server);
            DockContentOptimized dockContent = AddDockContent(server.DisplayName, server.NodeID, terminal, save: save);
            lock (_terminalControls)
            {
                _terminalControls.Add(terminal, dockContent);
            }
        }
        private void SaveSessions()
        {
            if (LoadingTab) return;
            if (Variables.ShutDownFired || !Variables.StartupComplete) return;

            _saveSessionsSemaphoreSlim.WaitUIFriendly();
            DockPanel_Main.SaveAsXml(Variables.SessionFile, Encoding.UTF8);
            _saveSessionsSemaphoreSlim.Release();
        }
        private void SendCommand(String command)
        {
            lock (_terminalControls)
            {
                Parallel.ForEach(_terminalControls, terminalControl =>
                {
                    terminalControl.Key.SendCommand(command);
                });
            }
        }

        private DockContentOptimized AddDockContent(String text, String internalName, Control content, DockState dockState = DockState.Document, Boolean save = true, Boolean add = true)
        {
            (UserControlOptimized userControlOptimized, DockState dockState, ToolStripMenuItem toolStripMenuItem, DockContentOptimized dockContentOptimized) result = (null, DockState.Unknown, null, null);
            if (_defaultUserControls.ContainsKey(internalName))
            {
                result = _defaultUserControls[internalName];
                if (result.dockContentOptimized != null)
                {
                    result.dockContentOptimized.Show(DockPanel_Main);
                    return result.dockContentOptimized;
                }

                dockState = result.dockState;
            }

            LoadingTab = true;

            DockContentOptimized newDockContent = new() { Text = text, Name = internalName, HideOnClose = _defaultUserControls.ContainsKey(internalName) };

            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(0);
            content.Margin = new Padding(0);

            newDockContent.Controls.Add(content);
            if (add)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<DockPanelOptimized, DockState>(newDockContent.Show), DockPanel_Main, dockState);
                }
                else
                {
                    newDockContent.Show(DockPanel_Main, dockState);
                }
            }

            newDockContent.SizeChanged += NewDockContentOnSizeChanged;
            newDockContent.DockStateChanged += (sender, _) => NewDockContentOnDockStateChanged((DockContentOptimized)sender, content);

            if (_defaultUserControls.ContainsKey(internalName))
            {
                result.dockContentOptimized = newDockContent;
                _defaultUserControls[internalName] = result;
            }

            LoadingTab = false;

            if (save)
            {
                SaveSessions();
            }
            
            return newDockContent;
        }
        private void NewDockContentOnSizeChanged(Object sender, EventArgs e)
        {
            if (!Variables.StartupComplete || Variables.ShutDownFired) return;

            SaveSessions();
        }
        private void NewDockContentOnDockStateChanged(DockContentOptimized dockContent, IDisposable content)
        {
            if (LoadingTab || !Variables.StartupComplete || Variables.ShutDownFired) return;
            
            if (dockContent.Controls.Count == 0 && content is TerminalControl terminalControl)
            {
                lock (_terminalControls)
                {
                    if (_terminalControls.ContainsKey(terminalControl))
                    {
                        _terminalControls.Remove(terminalControl);
                    }
                }
            }

            if (_defaultUserControls.ContainsKey(dockContent.Name))
            {
                _defaultUserControls[dockContent.Name].toolStripMenuItem.Checked = dockContent.Visible;
            }

            SaveSessions();
        }
    }
}
