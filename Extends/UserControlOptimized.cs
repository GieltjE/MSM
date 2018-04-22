using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using MSM.Functions;

namespace MSM.Extends
{
    [ToolboxBitmap(typeof(UserControl))]
    public class UserControlOptimized : UserControl
    {
        public UserControlOptimized()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BorderStyle = BorderStyle.FixedSingle;

            base.AutoScaleMode = AutoScaleMode.Dpi;
        }

        [DefaultValue(AutoScaleMode.Dpi)]
        public new AutoScaleMode AutoScaleMode => AutoScaleMode.Dpi;

        [DefaultValue(BorderStyle.FixedSingle)]
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