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
        private readonly Scene _sceneUpdater;

        public RenderWindowViewModel()
        {
            _view = new RenderWindowView()
            {
                DataContext = this,
            };

            Initialize();

            _renderer = new Renderer(this);
            _sceneUpdater = new Scene(_renderer);

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
            CreateBuffers(1024, 1024);
        }

        public async void MainLoop()
        {
            var previousTime = DateTime.UtcNow;
            var timeCounter = new TimeSpan();
            int frameCount = 0;

            bool allowCameraChange = true;

            while (true)
            {
                var cameraForwardMovement = 0.0;
                var cameraSideMovement = 0.0;
                var pawnForwardMovement = 0.0;
                var pawnSideMovement = 0.0;

                if (Keyboard.GetKeyStates(Key.W) == KeyStates.Down)
                {
                    cameraForwardMovement += 1.0;
                }

                if (Keyboard.GetKeyStates(Key.S) == KeyStates.Down)
                {
                    cameraForwardMovement -= 1.0;
                }

                if (Keyboard.GetKeyStates(Key.A) == KeyStates.Down)
                {
                    cameraSideMovement -= 1.0;
                }

                if (Keyboard.GetKeyStates(Key.D) == KeyStates.Down)
                {
                    cameraSideMovement += 1.0;
                }

                if (Keyboard.GetKeyStates(Key.T) == KeyStates.Down)
                {
                    pawnForwardMovement += 1.0;
                }

                if (Keyboard.GetKeyStates(Key.G) == KeyStates.Down)
                {
                    pawnForwardMovement -= 1.0;
                }

                if (Keyboard.GetKeyStates(Key.F) == KeyStates.Down)
                {
                    pawnSideMovement -= 1.0;
                }

                if (Keyboard.GetKeyStates(Key.H) == KeyStates.Down)
                {
                    pawnSideMovement += 1.0;
                }

                if (Keyboard.GetKeyStates(Key.Q) == KeyStates.Down)
                {
                    if (allowCameraChange)
                    {
                        _sceneUpdater.SwapCameras();
                    }
                    allowCameraChange = false;
                }
                else
                {
                    allowCameraChange = true;
                }

                _sceneUpdater.MoveCamera(cameraForwardMovement, cameraSideMovement);
                _sceneUpdater.MovePawn(pawnForwardMovement, pawnSideMovement);

                var currentTime = DateTime.UtcNow;
                var elapsedTime = currentTime - previousTime;
                previousTime = currentTime;

                timeCounter += elapsedTime;
                if (timeCounter.TotalSeconds > 1.0)
                {
                    _view.Title = "Software Renderer";// " FPS:" + frameCount + ")";
                    timeCounter = new TimeSpan(0, 0, 0, 0, timeCounter.Milliseconds);
                    frameCount = 0;
                }

                _sceneUpdater.Update(elapsedTime);
                _sceneUpdater.Render();
                frameCount++;

                await Task.Delay(1);
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
