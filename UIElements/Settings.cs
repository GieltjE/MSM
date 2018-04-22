using MSM.Extends;

namespace MSM.UIElements
{
    public partial class Settings : UserControlOptimized
    {
        public Settings()
        {
            InitializeComponent();

            PropertyGrid_Settings.SelectedObject = Service.Settings.Values;
        }
    }
}