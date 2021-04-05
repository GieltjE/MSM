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
using System.Reflection;
using System.Windows.Forms;
using MSM.Data;
using SolidBrush = System.Drawing.SolidBrush;

namespace MSM.Extends
{
    public class FormOptimized : Form
    {
        public FormOptimized(Form masterForm)
        {
            Owner = masterForm;
            Init();
        }
        public FormOptimized()
        {
            Init();
        }
        private void Init()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }

        private SolidBrush _backBrush;
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (DesignMode) return;

            _backBrush ??= new SolidBrush(Variables.ColorPalette.MainWindowActive.Background);
            e.Graphics.FillRectangle(_backBrush, DisplayRectangle);
        }
    }
}