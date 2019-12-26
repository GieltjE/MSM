using System.Drawing;
using System.Windows.Forms;
using MSM.Data;
using WeifenLuo.WinFormsUI.Docking;

namespace MSM.Extends
{
    internal class DockContentOptimized : DockContent
    {
        public DockContentOptimized()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            FormBorderStyle = FormBorderStyle.None;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using SolidBrush b = new SolidBrush(Variables.ColorPalette.MainWindowActive.Background);
            e.Graphics.FillRectangle(b, DisplayRectangle);
        }
    }
}