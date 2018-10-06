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
using System.Drawing;
using MSM.Data;
using MSM.Extends;

namespace MSM.UIElements
{
    public partial class Settings : UserControlOptimized
    {
        public Settings()
        {
            InitializeComponent();

            if (DesignMode || Variables.ColorPalette == null) return;

            PropertyGrid_Settings.SelectedObject = Service.Settings.Values;

            PropertyGrid_Settings.BackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
            PropertyGrid_Settings.CommandsBackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
            PropertyGrid_Settings.HelpBackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
            PropertyGrid_Settings.ViewBackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;

            PropertyGrid_Settings.CategorySplitterColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;

            Color c = Variables.ColorPalette.ToolWindowCaptionActive.Background;

            PropertyGrid_Settings.LineColor = Color.FromArgb(c.A, c.R + 10, c.G + 10, c.B + 10);

            PropertyGrid_Settings.CommandsBorderColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
            PropertyGrid_Settings.HelpBorderColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
            PropertyGrid_Settings.ViewBorderColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;

            PropertyGrid_Settings.ForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;
            PropertyGrid_Settings.CommandsForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;
            PropertyGrid_Settings.HelpForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;
            PropertyGrid_Settings.ViewForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;
            PropertyGrid_Settings.CategoryForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text; 
            PropertyGrid_Settings.SelectedItemWithFocusForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;
            PropertyGrid_Settings.DisabledItemForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;

            PropertyGrid_Settings.ExpandAllGridItems();
        }
    }
}