using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra;
using SoftwareRenderer.Helpers;
using SoftwareRenderer.Logic;
using System.Collections.Generic;
using System.Linq;

namespace SoftwareRenderer.Rendering
{
    public class Renderer : IUpdateable
    {
        private struct FramebufferData
        {
            public double Depth;
            public int ColorARGB;
            public double NormalX;
            public double NormalY;
            public double NormalZ;
        }

        private FramebufferData[,] _framebufferData;

        private readonly IRenderWindow _renderWindow;

        private readonly WireframeMesh _cubeWireframe;
        private readonly Mesh _cube;

        private readonly Camera _camera;

        private double _angle;

        public void InitializeBuffers()
        {
            var width = _renderWindow.Framebuffer.PixelWidth;
            var height = _renderWindow.Framebuffer.PixelHeight;
            _framebufferData = new FramebufferData[width, height];
        }

        public void ClearBuffers()
        {
        }

        public Renderer(IRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;

            _camera = new Camera();
            _cubeWireframe = MeshHelpers.GetCubeWireframe(1.0);
            _cube = MeshHelpers.GetCube(1.0);

            _camera.NearPlane = 0.1;
            _camera.FarPlane = 200.0;
            _camera.Position = VectorHelpers.Create(0, 0, -5);
            _camera.LookAt = VectorHelpers.Create(0, 0, 0);
            _camera.UpVector = VectorHelpers.Create(0, 1, 0);

            _clipper = new CohenSutherlandClipper();
        }

        public void Update(TimeSpan elapsedTime)
        {
            _angle += elapsedTime.TotalSeconds;
        }

        private CohenSutherlandClipper _clipper;

        public void DrawLine3D(WriteableBitmap rt, Matrix<double> transformation,
            Vector<double> from, Vector<double> to, double hw, double hh)
        {
            from = (transformation * from.ExtendVector()).ToCartesian();
            to = (transformation * to.ExtendVector()).ToCartesian();

            if (_clipper.Clip(from, to, out from, out to))
            {
                var fromx = (int) ((from[0] + 1)*hw);
                var fromy = (int) ((from[1] + 1)*hh);
                var tox = (int) ((to[0] + 1)*hw);
                var toy = (int) ((to[1] + 1)*hh);

                rt.DrawLine(fromx, fromy, tox, toy, Colors.White);
            }
        }

        public void RenderFrame()
        {
            var viewMatrix = _camera.GetViewMatrix();
            var projMatrix = _camera.GetProjectionMatrix();

            var modelMatrix = MatrixHelpers.RotationY(_angle);
            var projView = projMatrix * viewMatrix * modelMatrix;
   
            var rt = _renderWindow.Framebuffer;
            rt.Lock();
            rt.Clear(Colors.Black);

            var hw = rt.PixelWidth*0.5;
            var hh = rt.PixelHeight*0.5;

            for (var i = 0; i + 2 < _cube.Vertices.Count; i += 3)
            {
                var a = (projView * _cube.Vertices[i + 0].Position.ExtendVector()).ToCartesian()
                    .Add(1.0).PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0));
                var b = (projView * _cube.Vertices[i + 1].Position.ExtendVector()).ToCartesian()
                    .Add(1.0).PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0)); ;
                var c = (projView * _cube.Vertices[i + 2].Position.ExtendVector()).ToCartesian()
                    .Add(1.0).PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0)); ;

                DrawScreenSpaceTriangle(rt, new Vector<double>[] { a, b, c });

                //DrawLine3D(rt, projView, _cube.Vertices[i + 0].Position, _cube.Vertices[i + 1].Position, hw, hh);
                //DrawLine3D(rt, projView, _cube.Vertices[i + 1].Position, _cube.Vertices[i + 2].Position, hw, hh);
                //DrawLine3D(rt, projView, _cube.Vertices[i + 2].Position, _cube.Vertices[i + 0].Position, hw, hh);
            }

            rt.Unlock();
        }

        private class ScanEdge
        {
            public int MinimalY { get; set; }
            public int MaximalY { get; set; }
            public double X { get; set; }
            public double XSlope { get; set; }
        }

        public void DrawScreenSpaceTriangle(WriteableBitmap rt, Vector<double>[] triangle)
        {
            if (triangle.Length != 3)
            {
                throw new ApplicationException("This is not a triangle.");
            }

            var scanEdges = new List<ScanEdge>();

            for (int current = 0; current < 3; ++current)
            {
                int next = (current + 1) % 3;

                Vector<double> higher = triangle[current];
                Vector<double> lower = triangle[next];

                if (higher[1] > lower[1])
                {
                    higher = triangle[next];
                    lower = triangle[current];
                }

                int minY = (int)higher[1];
                int maxY = (int)lower[1];
                int startX = (int)higher[0];
                int endX = (int)lower[0];

                if (minY == maxY)
                {
                    continue;
                }

                var scanEdge = new ScanEdge();
                scanEdge.MinimalY = minY;
                scanEdge.MaximalY = maxY;
                scanEdge.X = startX;
                scanEdge.XSlope = (endX-startX)/((double)(maxY-minY));

                scanEdges.Add(scanEdge);
            }

            scanEdges = scanEdges.OrderBy(t => t.MinimalY).ToList();
            int scanEdgesActivated = 0;

            var startY = scanEdges.Min(t => t.MinimalY);
            var endY = scanEdges.Max(t => t.MaximalY);

            var activeEdges = new List<ScanEdge>();
            for (var y = startY; y <= endY; ++y)
            {
                activeEdges = activeEdges.Where(t => y < t.MaximalY).ToList();

                while (scanEdgesActivated < scanEdges.Count)
                {
                    if (y < scanEdges[scanEdgesActivated].MinimalY)
                    {
                        break;
                    }

                    activeEdges.Add(scanEdges[scanEdgesActivated]);
                    ++scanEdgesActivated;
                }

                activeEdges = activeEdges.OrderBy(t => t.X).ToList();

                for (int i = 0; i < activeEdges.Count - 1; i += 2)
                {
                    rt.DrawLine((int)activeEdges[i].X, y, (int)activeEdges[i + 1].X, y, Colors.White);
                }

                activeEdges.ForEach(t => t.X += t.XSlope);
            }

        }
    }
}
