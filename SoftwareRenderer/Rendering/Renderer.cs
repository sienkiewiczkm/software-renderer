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
        //private struct FramebufferData
        //{
        //    public double Depth;
        //    public int ColorARGB;
        //    public double NormalX;
        //    public double NormalY;
        //    public double NormalZ;
        //}

        //private FramebufferData[,] _framebufferData;

        private readonly IRenderWindow _renderWindow;

        //private readonly WireframeMesh _cubeWireframe;
        private readonly Mesh _cube;

        private readonly Camera _camera;

        private double _angle;

        private double[,] _zBuffer;

        public TriangleDirection VisibleTriangleDirection { get; set; }

        public ObjData _objData;

        public void InitializeBuffers()
        {
            var width = _renderWindow.Framebuffer.PixelWidth;
            var height = _renderWindow.Framebuffer.PixelHeight;
            //_framebufferData = new FramebufferData[width, height];
        }

        public void ClearBuffers()
        {
        }

        public Renderer(IRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;

            _camera = new Camera();
            //_cubeWireframe = MeshHelpers.GetCubeWireframe(1.0);
            _cube = MeshHelpers.GetCube(1.0);

            _camera.NearPlane = 0.1;
            _camera.FarPlane = 200.0;
            _camera.Position = VectorHelpers.Create(0, 5, -10);
            _camera.LookAt = VectorHelpers.Create(0, 0, 0);
            _camera.UpVector = VectorHelpers.Create(0, 1, 0);

            //_clipper = new CohenSutherlandClipper();

            VisibleTriangleDirection = TriangleDirection.Clockwise;

            _objData = ObjData.LoadFromFile("Data/Models/pawn.obj");
        }

        public void Update(TimeSpan elapsedTime)
        {
            _angle += elapsedTime.TotalSeconds;
        }

        //private CohenSutherlandClipper _clipper;

        //public void DrawLine3D(WriteableBitmap rt, Matrix<double> transformation,
        //    Vector<double> from, Vector<double> to, double hw, double hh)
        //{
        //    from = (transformation * from.ExtendVector()).ToCartesian();
        //    to = (transformation * to.ExtendVector()).ToCartesian();

        //    if (_clipper.Clip(from, to, out from, out to))
        //    {
        //        var fromx = (int) ((from[0] + 1)*hw);
        //        var fromy = (int) ((from[1] + 1)*hh);
        //        var tox = (int) ((to[0] + 1)*hw);
        //        var toy = (int) ((to[1] + 1)*hh);

        //        rt.DrawLine(fromx, fromy, tox, toy, Colors.White);
        //    }
        //}

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

            _zBuffer = new double[rt.PixelWidth, rt.PixelHeight];
            for (int y = 0; y < rt.PixelHeight; ++y)
            {
                for (int x = 0; x < rt.PixelWidth; ++x)
                {
                    _zBuffer[x, y] = Double.MaxValue;
                }
            }

            /*for (var i = 0; i + 2 < _cube.Vertices.Count; i += 3)
            {
                var a = (projView * _cube.Vertices[i + 0].Position.ExtendVector()).ToCartesian()
                    .Add(1.0).PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0));
                var b = (projView * _cube.Vertices[i + 1].Position.ExtendVector()).ToCartesian()
                    .Add(1.0).PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0)); ;
                var c = (projView * _cube.Vertices[i + 2].Position.ExtendVector()).ToCartesian()
                    .Add(1.0).PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0)); ;

                DrawScreenSpaceTriangle(rt, zbuffer, new Vector<double>[] { a, b, c });
            }*/

            RenderObjMesh(_objData, projView, modelMatrix);

            rt.Unlock();
        }

        private class ScanEdge
        {
            public int MinimalY { get; set; }
            public int MaximalY { get; set; }
            public double X { get; set; }
            public double XSlope { get; set; }
        }

        public void DrawScreenSpaceTriangle(WriteableBitmap rt, double[,] zbuffer, Vector<double>[] triangle)
        {
            if (triangle.Length != 3)
            {
                throw new ApplicationException("This is not a triangle.");
            }

            DrawScreenSpaceTriangleInterpolated(rt, zbuffer, triangle[0][0], triangle[0][1], triangle[0][2],
                triangle[1][0], triangle[1][1], triangle[1][2], triangle[2][0], triangle[2][1],
                triangle[2][2], Colors.Red, Colors.Blue, Colors.Green);
        }

        public void RenderObjMesh(ObjData meshToRender, Matrix<double> transformation, Matrix<double> normalTransformation)
        {
            var hw = _renderWindow.Framebuffer.PixelWidth * 0.5;
            var hh = _renderWindow.Framebuffer.PixelHeight * 0.5;

            var transformedMeshVertices = meshToRender.Vertices
                .Select(t => (transformation*t.ExtendVector()).ToCartesian().Add(1.0).PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0)))
                .ToList();

            var transformedMeshNormals = meshToRender.Normals
                .Select(t => (normalTransformation * t.ExtendVector()).ToCartesian().Normalize(2))
                .ToList();

            var sunlight = Vector<double>.Build.DenseOfArray(new[] { 1.0, 0.0, 0.0 });

            foreach (var triangle in meshToRender.Triangles)
            {
                var tri = new[] 
                { 
                    transformedMeshVertices[triangle.Vertices[0]],
                    transformedMeshVertices[triangle.Vertices[1]],
                    transformedMeshVertices[triangle.Vertices[2]] 
                };

                var light1 = Math.Max(sunlight.DotProduct(transformedMeshNormals[triangle.Normals[0]]), 0);
                var light2 = Math.Max(sunlight.DotProduct(transformedMeshNormals[triangle.Normals[1]]), 0);
                var light3 = Math.Max(sunlight.DotProduct(transformedMeshNormals[triangle.Normals[2]]), 0);

                var blight1 = (byte)(255 * light1);
                var blight2 = (byte)(255 * light2);
                var blight3 = (byte)(255 * light3);

                var c1 = Color.FromRgb(blight1, blight1, blight1);
                var c2 = Color.FromRgb(blight2, blight2, blight2);
                var c3 = Color.FromRgb(blight3, blight3, blight3);

                DrawScreenSpaceTriangleInterpolated(_renderWindow.Framebuffer, _zBuffer,
                    tri[0][0], tri[0][1], tri[0][2],
                    tri[1][0], tri[1][1], tri[1][2], 
                    tri[2][0], tri[2][1], tri[2][2],
                    c1, c2, c3);
            }
        }

        public enum TriangleDirection
        {
            Clockwise,
            CounterClockwise,
            Indeterminable,
        }

        public TriangleDirection Determine2DTriangleDirection(double ax, double ay, double bx, double by, 
            double cx, double cy)
        {
            var x1 = bx - ax;
            var y1 = by - ay;
            var x2 = cx - bx;
            var y2 = cy - by;

            double det = x1 * y2 - x2 * y1;

            if (Math.Abs(det) < Double.Epsilon)
            {
                return TriangleDirection.Indeterminable;
            }

            return det > 0 ? TriangleDirection.Clockwise : TriangleDirection.CounterClockwise;
        }

        public void DrawScreenSpaceTriangleInterpolated(WriteableBitmap rt, double[,] zbuffer,
            double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz,
            Color aColor, Color bColor, Color cColor)
        {
            if (Determine2DTriangleDirection(ax, ay, bx, by, cx, cy) != VisibleTriangleDirection)
            {
                return;
            }

            var triangleArea = CalculateTriangleArea(ax, ay, bx, by, cx, cy);

            var vertexX = new double[] { ax, bx, cx };
            var vertexY = new double[] { ay, by, cy };

            var scanEdges = new List<ScanEdge>();

            for (int current = 0; current < 3; ++current)
            {
                int next = (current + 1) % 3;

                double higherx, highery;
                double lowerx, lowery;

                if (vertexY[current] < vertexY[next])
                {
                    higherx = vertexX[current];
                    highery = vertexY[current];
                    lowerx = vertexX[next];
                    lowery = vertexY[next];
                }
                else
                {
                    higherx = vertexX[next];
                    highery = vertexY[next];
                    lowerx = vertexX[current];
                    lowery = vertexY[current];
                }

                int minY = (int)highery;
                int maxY = (int)lowery;
                int startX = (int)higherx;
                int endX = (int)lowerx;

                if (minY == maxY)
                {
                    continue;
                }

                var scanEdge = new ScanEdge();
                scanEdge.MinimalY = minY;
                scanEdge.MaximalY = maxY;
                scanEdge.X = startX;
                scanEdge.XSlope = (endX - startX) / ((double)(maxY - minY));

                scanEdges.Add(scanEdge);
            }

            if (scanEdges.Count == 0)
            {
                return;
            }

            scanEdges = scanEdges.OrderBy(t => t.MinimalY).ToList();
            int scanEdgesActivated = 0;

            var startY = Math.Max(scanEdges.Min(t => t.MinimalY), 0);
            var endY = Math.Min(scanEdges.Max(t => t.MaximalY), rt.PixelHeight-1);

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
                    int startX = Math.Max((int)activeEdges[i].X, 0);
                    int endX = Math.Min((int)activeEdges[i + 1].X, rt.PixelWidth-1);

                    for (int x = startX; x <= endX; ++x)
                    {
                        var aArea = CalculateTriangleArea(x, y, bx, by, cx, cy);
                        var bArea = CalculateTriangleArea(ax, ay, x, y, cx, cy);
                        var cArea = CalculateTriangleArea(ax, ay, bx, by, x, y);

                        var fa = Math.Min(aArea / triangleArea, 1.0);
                        var fb = Math.Min(bArea / triangleArea, 1.0);
                        var fc = Math.Min(cArea / triangleArea, 1.0);

                        var z = fa*az + fb*bz + fc*cz;

                        if (zbuffer[x, y] < z)
                        {
                            continue;
                        }

                        zbuffer[x, y] = z;

                        Color outputColor = new Color();
                        outputColor.A = 255;
                        outputColor.R = (byte)Math.Min((fa * aColor.R + fb * bColor.R + fc * cColor.R), 255.0);
                        outputColor.G = (byte)Math.Min((fa * aColor.G + fb * bColor.G + fc * cColor.G), 255.0);
                        outputColor.B = (byte)Math.Min((fa * aColor.B + fb * bColor.B + fc * cColor.B), 255.0);
                        rt.SetPixel(x, y, outputColor);
                    }
                }

                activeEdges.ForEach(t => t.X += t.XSlope);
            }
        }

        public double CalculateTriangleArea(double ax, double ay, double bx, double by, 
            double cx, double cy)
        {
            var determinant = ax*by + ay*cx + bx*cy - ax*cy - ay*bx - by*cx;
            return Math.Abs(determinant * 0.5);
        }
    }
}
