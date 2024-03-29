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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSM.Extends;

[DefaultProperty("CustomDropDownDirection"), ToolboxBitmap(typeof(ToolStripDropDownButton))]
public class ToolStripDropDownButtonOptimized : ToolStripDropDownButton
{
    [DefaultValue(ToolStripDropDownDirection.BelowRight), Description("Which direction to dropdown")]
    public ToolStripDropDownDirection CustomDropDownDirection
    {
        get => _customDropDownDirection;
        set
        {
            _customDropDownDirection = value;
            DropDownDirection = value;
        }
    }
    private ToolStripDropDownDirection _customDropDownDirection = ToolStripDropDownDirection.BelowRight;
}