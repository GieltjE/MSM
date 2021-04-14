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

using System.Drawing;
using System.Windows.Forms;
using MSM.Data;

namespace MSM.Extends
{
    [ToolboxBitmap(typeof(DataGridView))]
    public class DataGridViewOptimized : DataGridView
    {
        public DataGridViewOptimized()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

            CellBorderStyle = DataGridViewCellBorderStyle.RaisedVertical;
            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            BorderStyle = BorderStyle.None;
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            RowHeadersVisible = false;

            // ReSharper disable once RedundantBaseQualifier
            base.DoubleBuffered = true;
            // ReSharper disable once RedundantBaseQualifier
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            if (!Variables.DesignMode)
            {
                EnableHeadersVisualStyles = false;

                DefaultCellStyle.BackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
                AlternatingRowsDefaultCellStyle.BackColor = Variables.ColorPalette.ToolWindowCaptionActive.Grip;
                BackgroundColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
            }
        }
    }
}
