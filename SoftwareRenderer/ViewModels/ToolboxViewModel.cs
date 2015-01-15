using SoftwareRenderer.Views;

namespace SoftwareRenderer.ViewModels
{
    public class ToolboxViewModel
    {
        private ToolboxView _view;

        public ToolboxViewModel()
        {
            _view = new ToolboxView()
            {
                DataContext = this
            };

            _view.Show();
        }
    }
}
