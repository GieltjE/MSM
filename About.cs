// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2018-2022 Michiel Hazelhof (michiel@hazelhof.nl)
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MSM.Extends;
using MSM.Functions;

namespace MSM;

public partial class About : FormOptimized
{
    public About(Form owner) : base(owner)
    {
        InitializeComponent();

        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(FileOperations.GetCurrentExecutable());
        Version currentVersion = Version.Parse(fileVersionInfo.FileVersion);

        // ReSharper disable once VirtualMemberCallInConstructor
        Text += @" v" + currentVersion;

        String licenseFile = Path.Combine(FileOperations.GetRunningDirectory(), "LICENSE.md");
        if (File.Exists(licenseFile))
        {
            RichTextBox_License.Text = File.ReadAllText(licenseFile);
        }
    }
}