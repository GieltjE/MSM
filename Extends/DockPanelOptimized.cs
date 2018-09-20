using System.Windows.Forms;
using MSM.Data;
using WeifenLuo.WinFormsUI.Docking;

namespace MSM.Extends
{
    internal class DockPanelOptimized : DockPanel
    {
        public DockPanelOptimized()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            if (!DesignMode && Variables.ColorPalette != null)
            {
                // ReSharper disable once RedundantBaseQualifier
                base.BackColor = Variables.ColorPalette.DockTarget.Background;
            }
        }
    }
}