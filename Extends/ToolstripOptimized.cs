// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2018 Michiel Hazelhof (michiel@hazelhof.nl)
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

namespace MSM.Extends
{
    [ToolboxBitmap(typeof(ToolStrip))]
    public class ToolstripOptimized : ToolStrip
    {
        public ToolstripOptimized()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            Renderer = new ToolStripProfessionalRendererNoLine(CustomColorTableStatusStrip);
        }

        public CustomColorTableStatusStrip CustomColorTableStatusStrip = new CustomColorTableStatusStrip();
    }

    internal class ToolStripProfessionalRendererNoLine : ToolStripProfessionalRenderer
    {
        public ToolStripProfessionalRendererNoLine(ProfessionalColorTable table) : base(table) { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
    }
    public class CustomColorTableStatusStrip : ProfessionalColorTable
    {
        public CustomColorTableStatusStrip()
        {
            BackGroundGradientBegin = base.StatusStripGradientBegin;
            BackGroundGradientEnd = base.StatusStripGradientEnd;
        }

        public override Color StatusStripGradientBegin => BackGroundGradientBegin;
        public override Color StatusStripGradientEnd => BackGroundGradientEnd;
        public Color BackGroundGradientBegin;
        public Color BackGroundGradientEnd;
    }
}