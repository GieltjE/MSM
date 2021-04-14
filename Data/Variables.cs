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
using System.IO;
using System.Windows.Forms;
using MSM.Functions;
using WeifenLuo.WinFormsUI.Docking;

namespace MSM.Data
{
    public static class Variables
    {
        public static Int32 ThreadAfterDoEventsSleep = 50;
        public static Main MainForm;
        public static DockPanelColorPalette ColorPalette;
        public static Measures Measures;
        public static Boolean ShutDownFired;
        public static Boolean StartupComplete;
        public static String SettingsDirectory => Path.Combine(FileOperations.GetRunningDirectory(), "settings");
        public static String SessionFile => Path.Combine(SettingsDirectory, "PreviousSessions.xml");
        public static String PortableSettingsFile => Path.Combine(SettingsDirectory, "Settings.xml");
        public static String NormalSettingsFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSM", "Settings.xml");
        public static String SettingsFileChosen { get; set; }
        public static String LogDirectory => Path.Combine(FileOperations.GetRunningDirectory(), "logs");
        public static Int32 MainSTAThreadID;

        private static Boolean? _designMode;
        public static Boolean DesignMode
        {
            get
            {
                _designMode ??= Application.ExecutablePath.EndsWith("devenv.exe");
                return (Boolean)_designMode;
            }
        }
    }
}
