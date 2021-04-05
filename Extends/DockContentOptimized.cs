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
            Padding = new Padding(0);
        }

        private readonly SolidBrush _brush = new(Variables.ColorPalette.MainWindowActive.Background);
        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.FillRectangle(_brush, DisplayRectangle);
    }
}