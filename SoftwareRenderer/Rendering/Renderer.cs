using System.Windows.Media;
using System.Windows.Media.Imaging;
using SoftwareRenderer.Helpers;

namespace SoftwareRenderer.Rendering
{
    public class Renderer
    {
        private readonly IRenderWindow _renderWindow;

        private readonly WireframeMesh _cubeMesh;
        private readonly Camera _camera;

        public Renderer(IRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;

            _camera = new Camera();
            _cubeMesh = MeshHelpers.GetCubeWireframe(2.0);

            _camera.Position = VectorHelpers.Create(3, 3, 5);
            _camera.LookAt = VectorHelpers.Create(0, 0, 0);
            _camera.UpVector = VectorHelpers.Create(0, 1, 0);
        }

        public void RenderFrame()
        {
            var viewMatrix = _camera.GetViewMatrix();
            var projMatrix = _camera.GetProjectionMatrix();

            var projView = projMatrix * viewMatrix;

            var rt = _renderWindow.Framebuffer;
            rt.Lock();
            rt.Clear(Colors.Black);

            var hw = rt.PixelWidth*0.5;
            var hh = rt.PixelHeight*0.5;

            foreach (var edge in _cubeMesh.Edges)
            {
                var from = (projView * edge.From.ExtendVector()).ToCartesian();
                var to = (projView * edge.To.ExtendVector()).ToCartesian();

                if (from[0] < -1 || from[0] > 1 || from[1] < -1 || from[1] > 1)
                    continue;

                if (to[0] < -1 || to[0] > 1 || to[1] < -1 || to[1] > 1)
                    continue;

                var fromx = (int)((from[0] + 1) * hw);
                var fromy = (int)((from[1] + 1) * hh);
                var tox = (int)((to[0] + 1) * hw);
                var toy = (int)((to[1] + 1) * hh);

                rt.DrawLine(fromx, fromy, tox, toy, Colors.White);
            }

            rt.Unlock();
        }
    }
}
