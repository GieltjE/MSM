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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using MSM.Data;
using MSM.Functions;

namespace MSM.Extends
{
    [ToolboxBitmap(typeof(UserControl))]
    public class UserControlOptimized : UserControl
    {
        public UserControlOptimized()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BorderStyle = BorderStyle.None;


            if (!DesignMode)
            {
                // ReSharper disable RedundantBaseQualifier
                base.BackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
                base.ForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;
                // ReSharper restore RedundantBaseQualifier
            }


            base.AutoScaleMode = AutoScaleMode.Dpi;
        }

        [DefaultValue(AutoScaleMode.Dpi)]
        public new AutoScaleMode AutoScaleMode => AutoScaleMode.Dpi;

        [DefaultValue(BorderStyle.None)]
        public new BorderStyle BorderStyle { get => base.BorderStyle; set => base.BorderStyle = value; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            base.OnPaint(e);
        }

        public event ExtensionMethods.CustomDelegate OnDisposeEvent;
        protected override void Dispose(Boolean disposing)
        {
            try
            {
                OnDisposeEvent?.Invoke();
                base.Dispose(disposing);
            }
            catch {}
        }

        public event ExtensionMethods.CustomDelegate OnKeyEvent;
        protected Keys PressedKey;
        public void SendKey(Keys key)
        {
            PressedKey = key;
            OnKeyEvent?.Invoke();
        }
    }
}