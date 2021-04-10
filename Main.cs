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

        private readonly VisualStudioToolStripExtender _visualStudioToolStripExtender = new();

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
            Service.Events.ProcessExited += EventsOnProcessExited;

            if (Settings.Values.MaximizeOnStart)
            {
                WindowState = FormWindowState.Maximized;
            }

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
            Visible = false;

            FileOperations.CreateDirectory(Variables.SettingsDirectory);

            Boolean loadSuccess = false;
            if (File.Exists(Variables.SessionFile))
            {
                try
                {
                    DockPanel_Main.LoadFromXml(Variables.SessionFile, GetContentFromPersistString);
                    
                    foreach (DockPane dockPane in DockPanel_Main.Panes)
                    {
                        foreach (IDockContent content in dockPane.Contents.Reverse())
                        {
                            dockPane.ActiveContent = content;
                        }
                    }
                    loadSuccess = true;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch {}
            }
            if (!loadSuccess)
            {
                ToolStripShowServerListClick(null, null);
            }

            if (Settings.Values.InitialSessions != Enumerations.InitialSessions.Predefined)
            {
                Visible = true;
                return;
            }

            IOrderedEnumerable<Server> orderedList = Settings.AllServers.Values.Where(allServersValue => allServersValue.PredefinedStartIndex != -1).OrderBy(x => x.PredefinedStartIndex);
            foreach (Server server in orderedList)
            {
                AddServer(server, false);
            }

            Visible = true;
        }
        private IDockContent GetContentFromPersistString(String persistString)
        {
            Control content;
            String displayName, internalName;
            Boolean allowDuplicate = true;
            if (String.Equals(persistString, "Serverlist", StringComparison.Ordinal))
            {
                ToolStrip_ShowServerList.Checked = true;
                content = new Servers();
                internalName = "Serverlist";
                displayName = "Serverlist";
                allowDuplicate = false;
            }
            else if (String.Equals(persistString, "Settings", StringComparison.Ordinal))
            {
                ToolStrip_Settings.Checked = true;
                content = new UIElements.Settings();
                internalName = "Settings";
                displayName = "Settings";
                allowDuplicate = false;
            }
            else
            {
                if (Settings.Values.InitialSessions != Enumerations.InitialSessions.Previous) return null;

                Server server = Settings.FindServer(persistString);
                if (server == null) return null;

                content = new Terminal(server);
                internalName = server.NodeID;
                displayName = server.DisplayName;
            }

            return AddDockContent(displayName, internalName, content, allowDuplicate, DockState.Float, false, false);
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
            Service.Events.ShutDownFired -= ShutDownFired;
            Service.Events.ProcessExited -= EventsOnProcessExited;
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

        private void ToolStripMenuItemAboutClick(Object sender, EventArgs e)
        {
            new About(this).Show();
        }
        private void ToolStripMenuItemCheckForUpdateClick(Object sender, EventArgs e)
        {
            UpdateCheck.TriggerUpdateCheckJob();
        }
        private void ToolStripSettingsClick(Object sender, EventArgs e)
        {
            if (!ToolStrip_Settings.Checked)
            {
                ToolStrip_Settings.Checked = true;
                AddDockContent("Settings", "Settings", new UIElements.Settings(), false);
            }
            else
            {
                HideDockContent("Settings");
            }
        }
        private void ToolStripShowServerListClick(Object sender, EventArgs e)
        {
            if (!ToolStrip_ShowServerList.Checked)
            {
                ToolStrip_ShowServerList.Checked = true;
                AddDockContent("Serverlist", "Serverlist", new Servers(), false, DockState.DockRight);
            }
            else
            {
                HideDockContent("Serverlist");
            }
        }

        private void EventsOnProcessExited(Terminal terminal)
        {
            if (!Settings.Values.CloseTabOnCrash) return;
            if (InvokeRequired)
            {
                Invoke(new Action<Terminal>(EventsOnProcessExited), terminal);
                return;
            }
            foreach (DockPane dockPane in DockPanel_Main.Panes)
            {
                foreach (IDockContent dockPaneContent in dockPane.Contents)
                {
                    if (dockPaneContent is not DockContentOptimized dockContent || dockContent.Controls[0] != terminal) continue;
                    dockContent.Close();
                    return;
                }
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

            Terminal terminal = new(server);
            AddDockContent(server.DisplayName, server.NodeID, terminal, true, save: save);
        }
        private void SaveSessions()
        {
            if (Variables.ShutDownFired) return;
            DockPanel_Main.SaveAsXml(Variables.SessionFile, Encoding.UTF8);
        }

        private readonly Dictionary<String, DockContentOptimized> _availableDocks = new(StringComparer.Ordinal);
        private DockContentOptimized AddDockContent(String text, String internalName, Control content, Boolean allowDuplicate, DockState dockState = DockState.Document, Boolean save = true, Boolean add = true)
        {
            if (!allowDuplicate && _availableDocks.ContainsKey(internalName))
            {
                _availableDocks[internalName].Show(DockPanel_Main, dockState);
                return _availableDocks[internalName];
            }

            DockContentOptimized newDockContent = new() { Text = text, Name = internalName };

            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(0);
            content.Margin = new Padding(0);

            newDockContent.Controls.Add(content);
            if (add)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action<DockPanelOptimized, DockState>(newDockContent.Show), DockPanel_Main, dockState).AutoEndInvoke(this);
                }
                else
                {
                    newDockContent.Show(DockPanel_Main, dockState);
                }
            }

            if (!allowDuplicate)
            {
                _availableDocks.Add(internalName, newDockContent);
            }

            if (save)
            {
                SaveSessions();
            }

            return newDockContent;
        }
        private void DockPanelMainContentRemoved(Object sender, DockContentEventArgs e)
        {
            if (e.Content is not DockContentOptimized activeContent) return;
            if (!_availableDocks.ContainsKey(activeContent.Name)) return;

            if (String.Equals(activeContent.Name, "Serverlist", StringComparison.Ordinal))
            {
                ToolStrip_ShowServerList.Checked = false;
            }
            else if (String.Equals(activeContent.Name, "Settings", StringComparison.Ordinal))
            {
                ToolStrip_Settings.Checked = false;
            }

            if (_availableDocks.ContainsKey(activeContent.Name))
            {
                DockContentOptimized item = _availableDocks[activeContent.Name];
                item?.Dispose();
                _availableDocks.Remove(activeContent.Name);
            }

            SaveSessions();
        }
        private void HideDockContent(String internalName)
        {
            if (!_availableDocks.ContainsKey(internalName)) return;

            if (_availableDocks[internalName].IsDisposed)
            {
                _availableDocks.Remove(internalName);
                return;
            }
            
            _availableDocks[internalName].Close();
        }
    }
}