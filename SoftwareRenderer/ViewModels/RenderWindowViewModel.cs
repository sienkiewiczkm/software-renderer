using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftwareRenderer.Views;

namespace SoftwareRenderer.ViewModels
{
	public class RenderWindowViewModel
	{
		private RenderWindowView _view;

		public RenderWindowViewModel()
		{
			_view = new RenderWindowView()
			{
				DataContext = this,
			};

			_view.Show();
		}
	}
}
