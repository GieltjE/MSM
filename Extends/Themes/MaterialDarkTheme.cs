// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2022 Michiel Hazelhof (michiel@hazelhof.nl)
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

using WeifenLuo.WinFormsUI.Docking;

namespace MSM.Extends.Themes;

public class MaterialDarkTheme : VS2015DarkTheme
{
    public MaterialDarkTheme()
    {
        System.Drawing.Color background = System.Drawing.Color.FromArgb(255, 27, 27, 27);
        System.Drawing.Color text = System.Drawing.Color.FromArgb(255, 200, 200, 200);

        ColorPalette.MainWindowActive.Background = background;
        ColorPalette.DockTarget.Background = background;
        ColorPalette.MainWindowStatusBarDefault.Background = background;
        ColorPalette.ToolWindowCaptionButtonActiveHovered.Background = background;
        ColorPalette.ToolWindowCaptionButtonInactiveHovered.Background = background;
        ColorPalette.ToolWindowCaptionButtonPressed.Background = background;

        ColorPalette.ToolWindowCaptionActive.Background = background;
        ColorPalette.ToolWindowCaptionActive.Grip = background;
        ColorPalette.ToolWindowCaptionActive.Button = background;
        ColorPalette.ToolWindowCaptionActive.Text = text;

        ColorPalette.ToolWindowCaptionInactive.Background = background;
        ColorPalette.ToolWindowCaptionInactive.Grip = background;
        ColorPalette.ToolWindowCaptionInactive.Text = text;

        ColorPalette.ToolWindowTabSelectedActive.Background = background;
        ColorPalette.ToolWindowTabSelectedInactive.Background = background;
        ColorPalette.ToolWindowTabUnselected.Background = background;
        ColorPalette.ToolWindowTabUnselectedHovered.Background = background;
        ColorPalette.ToolWindowBorder = background;
    }
}