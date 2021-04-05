// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2018 Michiel Hazelhof (michiel@hazelhof.nl)
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
using System.Drawing;
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;
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
            InitializeComponent();

            DockPanel_Main.Theme = new VS2015BlueTheme();

            Variables.MainForm = this;
            Variables.ColorPalette = DockPanel_Main.Theme.ColorPalette;
            Variables.Measures = DockPanel_Main.Theme.Measures;

            _visualStudioToolStripExtender.SetStyle(ToolStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel_Main.Theme);
            _visualStudioToolStripExtender.SetStyle(StatusStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel_Main.Theme);

            if (Settings.Values.CheckForUpdates)
            {
                UpdateCheck.StartUpdateCheck();
            }

            Service.Events.ShutDownFired += ShutDownFired;

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

            if (Settings.Values.ShowServerList)
            {
                ToolStrip_ShowServerList.Checked = true;
                ToolStripShowServerListClick(null, null);
            }
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

        private void ShutDownFired()
        {
            NotifyIcon.Visible = false;
        }
        
        private void ToolStripMenuItemAboutClick(Object sender, EventArgs e)
        {
            new About(this).Show();
        }
        private void ToolStripSettingsClick(Object sender, EventArgs e)
        {
            AddDockContent("Settings", "Settings", new UIElements.Settings(), false);
        }
        private void ToolStripShowServerListClick(Object sender, EventArgs e)
        {
            Settings.Values.ShowServerList = ToolStrip_ShowServerList.Checked;
            if (Settings.Values.ShowServerList)
            {
                DockContent dockContent = AddDockContent("Serverlist", "Serverlist", new Servers { Width = 100}, false, DockState.DockRight);
                dockContent.Width = Settings.Values.ServerListWidth;
                dockContent.Closing += ServerListClosing;
                dockContent.SizeChanged += (_, _) => { Settings.Values.ServerListWidth = dockContent.Width; };
            }
            else
            {
                HideDockContent("Serverlist", false);
            }
        }
        private void ServerListClosing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolStrip_ShowServerList.Checked = false;
        }

        public void AddServer(Server server)
        {
            if (server == null) return;

            ThreadHelpers thread = new();
            thread.ExecuteThreadParameter(AddServerHelper, server, false, false);
        }
        private void AddServerHelper(Object server)
        {
            Terminal terminal = new((Server)server);
            AddDockContent(((Server)server).DisplayName, ((Server)server).NodeID, terminal, true);
        }

        private readonly Dictionary<String, DockContentOptimized> _availableDocks = new(StringComparer.Ordinal);
        private DockContentOptimized AddDockContent(String text, String internalName, Control content, Boolean allowDuplicate, DockState dockState = DockState.Document)
        {
            if (!allowDuplicate && _availableDocks.ContainsKey(internalName))
            {
                _availableDocks[internalName].Show();
                _availableDocks[internalName].BringToFront();
                return _availableDocks[internalName];
            }

            DockContentOptimized newDockContent = new() { Text = text, Name = internalName };

            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(0);
            content.Margin = new Padding(0);

            newDockContent.Controls.Add(content);
            if (InvokeRequired)
            {
                BeginInvoke(new Action<DockPanelOptimized, DockState>(newDockContent.Show), DockPanel_Main, dockState).AutoEndInvoke(this);
            }
            else
            {
                newDockContent.Show(DockPanel_Main, dockState);
            }

            if (!allowDuplicate)
            {
                _availableDocks.Add(internalName, newDockContent);
            }

            return newDockContent;
        }
        private void HideDockContent(String internalName, Boolean remove)
        {
            if (!_availableDocks.ContainsKey(internalName)) return;

            if (_availableDocks[internalName].IsDisposed)
            {
                _availableDocks.Remove(internalName);
                return;
            }

            if (!remove)
            {
                _availableDocks[internalName].Hide();
            }
            else
            {
                _availableDocks[internalName].Close();
                _availableDocks[internalName].Dispose();
                _availableDocks.Remove(internalName);
            }
        }
    }
}