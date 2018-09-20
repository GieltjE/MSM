using WeifenLuo.WinFormsUI.Docking;

namespace MSM.Extends.Themes
{
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
}