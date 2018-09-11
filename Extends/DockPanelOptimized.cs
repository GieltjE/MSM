using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSM.Data;
using WeifenLuo.WinFormsUI.Docking;

namespace MSM.Extends
{
    class DockPanelOptimized : DockPanel
    {
        public DockPanelOptimized(): base()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            base.BackColor = Variables.ColorPalette.DockTarget.Background;
        }
    }
}
