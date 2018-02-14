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
		}

        private readonly VisualStudioToolStripExtender _visualStudioToolStripExtender = new VisualStudioToolStripExtender();

        private void MainFormClosing(Object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;

			Properties.Settings.Default.Save();

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
			string theme = "";

            if (sender == ToolStripMenuItem_Light)
            {
                DockPanel.Theme = new VS2015LightTheme();
				theme = "light";
            }
            else if (sender == ToolStripMenuItem_Blue)
            {
                DockPanel.Theme = new VS2015BlueTheme();
				theme = "blue";
			}
            else if (sender == ToolStripMenuItem_Dark)
            {
                DockPanel.Theme = new VS2015DarkTheme();
				theme = "dark";
			}
            _visualStudioToolStripExtender.SetStyle(ToolStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel.Theme);
            _visualStudioToolStripExtender.SetStyle(StatusStrip, VisualStudioToolStripExtender.VsVersion.Vs2015, DockPanel.Theme);

			if (theme != "")
			{
				Properties.Settings.Default.theme = theme;
			}
        }

		private void SetSize(object sender, EventArgs e)
		{
			Properties.Settings.Default.size = Size;
		}

		private void LoadHandler(object sender, EventArgs e)
		{
			Size = Properties.Settings.Default.size;

			switch (Properties.Settings.Default.theme)
			{
				case "light":
					DockPanel.Theme = new VS2015LightTheme();
					break;

				case "blue":
					DockPanel.Theme = new VS2015BlueTheme();
					break;

				case "dark":
					DockPanel.Theme = new VS2015DarkTheme();
					break;
			}
		}
	}
}