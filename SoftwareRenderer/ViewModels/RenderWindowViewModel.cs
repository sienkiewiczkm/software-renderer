using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SoftwareRenderer.Logic;
using SoftwareRenderer.Rendering;
using SoftwareRenderer.Views;

namespace SoftwareRenderer.ViewModels
{
    public class RenderWindowViewModel : INotifyPropertyChanged, IRenderWindow
    {
        private readonly RenderWindowView _view;
        private ToolboxViewModel _toolboxViewModel;

        private readonly Renderer _renderer;
        private readonly IUpdateable _sceneUpdater;

        public RenderWindowViewModel()
        {
            _view = new RenderWindowView()
            {
                DataContext = this,
            };

            _renderer = new Renderer(this);
            Initialize();

            _sceneUpdater = _renderer;

            _view.Show();

            MainLoop();
        }

        private WriteableBitmap _framebuffer;
        public WriteableBitmap Framebuffer
        {
            get { return _framebuffer; }
            set
            {
                _framebuffer = value;
                OnPropertyChanged();
            }
        }

        public void CreateBuffers(int pixelWidth, int pixelHeight)
        {
            _view.RenderTarget.Width = pixelWidth;
            _view.RenderTarget.Height = pixelHeight;

            var framebuffer = BitmapFactory.New(pixelWidth, pixelHeight);
            framebuffer.FillRectangle(0, 0, pixelWidth, pixelHeight, Colors.Black);
            Framebuffer = framebuffer;
        }

        public void Initialize()
        {
            CreateBuffers(512, 512);
        }

        private bool _once;

        public async void MainLoop()
        {
            var previousTime = DateTime.UtcNow;
            while (true)
            {
                var currentTime = DateTime.UtcNow;
                var elapsedTime = currentTime - previousTime;
                previousTime = currentTime;

                _sceneUpdater.Update(elapsedTime);
                _renderer.RenderFrame();
                await Task.Delay(5);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
