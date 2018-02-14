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
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;
using MSM.Service;
using WeifenLuo.WinFormsUI.Docking;

namespace MSM
{
    public partial class Main : FormOptimized
    {
        public Main()
        {
            Variables.MainForm = this;
            InitializeComponent();

            if (Settings.SettingsClass.CheckForUpdates)
            {
                UpdateCheck.StartUpdateCheck();
            }

            SetThemeHelper(Settings.SettingsClass.Theme);
		}

        private readonly VisualStudioToolStripExtender _visualStudioToolStripExtender = new VisualStudioToolStripExtender();

        private void MainFormClosing(Object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;

			Service.Events.ShutDown();
            e.Cancel = true;
        }

        private void ToolStripMenuItemAboutClick(Object sender, EventArgs e)
        {
            About about = new About(this);
            about.Show();
        }

        private void SetTheme(Object sender, EventArgs e)
        {
            if (sender == ToolStripMenuItem_Light)
            {
                Settings.SettingsClass.Theme = Enumerations.Themes.Light;
            }
            else if (sender == ToolStripMenuItem_Blue)
            {
                Settings.SettingsClass.Theme = Enumerations.Themes.Blue;
            }
            else if (sender == ToolStripMenuItem_Dark)
            {
                Settings.SettingsClass.Theme = Enumerations.Themes.Dark;
            }
            SetThemeHelper(Settings.SettingsClass.Theme);
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