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
using System.Drawing;
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;
using MSM.Service;
using WeifenLuo.WinFormsUI.Docking;

namespace MSM
{
    public partial class Main : FormOptimized
    {
        public readonly NotifyIcon NotifyIcon = new NotifyIcon();
        private readonly ContextMenuStrip _contextMenuStripTrayIcon = new ContextMenuStrip();
        private readonly ToolStripMenuItem _trayIconStripOpen = new ToolStripMenuItem();
        private readonly ToolStripMenuItem _trayIconStripExit = new ToolStripMenuItem();
        private FormWindowState _previousWindowState = FormWindowState.Normal;

        private readonly VisualStudioToolStripExtender _visualStudioToolStripExtender = new VisualStudioToolStripExtender();

        public Main()
        {
            Variables.MainForm = this;
            InitializeComponent();

            if (Settings.Values.CheckForUpdates)
            {
                UpdateCheck.StartUpdateCheck();
            }

            SetThemeHelper(Settings.Values.Theme);

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

            _trayIconStripOpen.Text = "Open";
            _trayIconStripOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _trayIconStripOpen.Click += TrayIconStripOpenClick;

            _trayIconStripExit.Text = "Close";
            _trayIconStripExit.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _trayIconStripExit.Click += TrayIconStripExitClick;

            _contextMenuStripTrayIcon.Items.AddRange(new ToolStripItem[] { _trayIconStripOpen, _trayIconStripExit });

            NotifyIcon.Visible = Settings.Values.AlwaysShowTrayIcon;
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
            if (!(e is MouseEventArgs) || ((MouseEventArgs)e).Button != MouseButtons.Left) return;

            TrayIconStripOpenClick(null, null);
        }
        private void TrayIconMouseClick(Object sender, MouseEventArgs e)
        {
            if (e == null || e.Button != MouseButtons.Right) return;

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

        private void AddDockContent(String text, String internalName, Control content, Boolean allowDuplicate)
        {
            if (!allowDuplicate)
            {
                foreach (IDockContent dockPanelContent in DockPanel.Contents)
                {
                    if (dockPanelContent.GetType() != typeof(DockContent)) continue;
                    if (!String.Equals(((DockContent)dockPanelContent).Name, internalName, StringComparison.Ordinal)) continue;

                    ((DockContent)dockPanelContent).BringToFront();

                    return;
                }
            }

            DockContent newDockContent = new DockContent { Text = text, Name = internalName };

            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(0);
            content.Margin = new Padding(0);

            newDockContent.Controls.Add(content);
            newDockContent.Show(DockPanel, DockState.Document);
        }
        private void SetTheme(Object sender, EventArgs e)
        {
            if (sender == ToolStripMenuItem_Light)
            {
                Settings.Values.Theme = Enumerations.Themes.Light;
            }
            else if (sender == ToolStripMenuItem_Blue)
            {
                Settings.Values.Theme = Enumerations.Themes.Blue;
            }
            else if (sender == ToolStripMenuItem_Dark)
            {
                Settings.Values.Theme = Enumerations.Themes.Dark;
            }
            SetThemeHelper(Settings.Values.Theme);
        }
        private void SetThemeHelper(Enumerations.Themes theme)
        {
            switch (theme)
            {
                case Enumerations.Themes.Light:
                    DockPanel.Theme = new VS2015LightTheme();
                    break;
                case Enumerations.Themes.Blue:
                    DockPanel.Theme = new VS2015BlueTheme();
                    break;
                case Enumerations.Themes.Dark:
                    DockPanel.Theme = new VS2015DarkTheme();
                    break;
            }
            _visualStudioToolStripExtender.SetStyle(ToolStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel.Theme);
            _visualStudioToolStripExtender.SetStyle(StatusStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel.Theme);

            Settings.Flush();
        }
    }
}