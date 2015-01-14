using System.Windows;
using SoftwareRenderer.ViewModels;

namespace SoftwareRenderer
{
	public partial class App : Application
	{
		private void OnStartup(object sender, StartupEventArgs e)
		{
			var renderWindow = new RenderWindowViewModel();
		}
	}
}
