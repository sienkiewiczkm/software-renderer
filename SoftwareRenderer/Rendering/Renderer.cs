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
    public enum ScreenSpaceTriangleDirection
    {
        Clockwise,
        CounterClockwise,
        Indeterminable,
    }

    public class PreparedVertex
    {
        public double[] Coordinates { get; protected set; }
        public double X { get { return Coordinates[0]; } set { Coordinates[0] = value; } }
        public double Y { get { return Coordinates[1]; } set { Coordinates[1] = value; } }
        public double Z { get { return Coordinates[2]; } set { Coordinates[2] = value; } }

        public Color VertexColor { get; set; }

        public PreparedVertex()
        {
            Coordinates = new double[3];
        }

        public void SetCoordinates(Vector<double> vector)
        {
            if (vector.Count < 3)
            {
                throw new ArgumentException("Vector must have at least 3 dimmensions.");
            }

            X = vector[0];
            Y = vector[1];
            Z = vector[2];
        }
    }

    public class PreparedTriangle
    {
        public PreparedVertex[] Vertices { get; protected set; }

        public PreparedTriangle()
        {
            Vertices = new PreparedVertex[3];
            for (int i = 0; i < 3; ++i)
            {
                Vertices[i] = new PreparedVertex();
            }
        }

        public ScreenSpaceTriangleDirection GetScreenSpaceDirection()
        {
            var x1 = Vertices[1].X - Vertices[0].X;
            var y1 = Vertices[1].Y - Vertices[0].Y;
            var x2 = Vertices[2].X - Vertices[1].X;
            var y2 = Vertices[2].Y - Vertices[1].Y;

            double det = x1 * y2 - x2 * y1;

            if (Math.Abs(det) < Double.Epsilon)
            {
                return ScreenSpaceTriangleDirection.Indeterminable;
            }

            return det > 0 ? ScreenSpaceTriangleDirection.Clockwise : ScreenSpaceTriangleDirection.CounterClockwise;
        }
    }

    public class Renderer : IUpdateable
    {
        private readonly IRenderWindow _renderWindow;
        private double[,] _zBuffer;

        private readonly Mesh _cube;

        public Camera Camera { get; set; }

        private double _angle;       

        public ScreenSpaceTriangleDirection VisibleTriangleDirection { get; set; }

        public ObjData _objData;

        public void InitializeBuffers()
        {
            var width = _renderWindow.Framebuffer.PixelWidth;
            var height = _renderWindow.Framebuffer.PixelHeight;
        }

        public void ClearBuffers()
        {
        }

        public Renderer(IRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;

            Camera = new Camera();
            _cube = MeshHelpers.GetCube(1.0);

            Camera.NearPlane = 0.1;
            Camera.FarPlane = 200.0;
            Camera.Position = VectorHelpers.Create(0, 5, -10);
            Camera.LookAt = VectorHelpers.Create(0, 0, 0);
            Camera.UpVector = VectorHelpers.Create(0, 1, 0);

            VisibleTriangleDirection = ScreenSpaceTriangleDirection.Clockwise;

            _objData = ObjData.LoadFromFile("Data/Models/pawn.obj");
        }

        public void Update(TimeSpan elapsedTime)
        {
            _angle += elapsedTime.TotalSeconds;
        }

        public void RenderFrame()
        {
            var viewMatrix = Camera.GetViewMatrix();
            var projMatrix = Camera.GetProjectionMatrix();

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

            RenderObjMesh(_objData, projView, modelMatrix);

            rt.Unlock();
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
                var tri = new PreparedTriangle();

                for (int i = 0; i < 3; ++i)
                {
                    // Vertex shader
                    tri.Vertices[i].SetCoordinates(transformedMeshVertices[triangle.Vertices[i]]);
                    var light = Math.Max(sunlight.DotProduct(transformedMeshNormals[triangle.Normals[i]]), 0);
                    var blight = (byte)(255 * light);
                    tri.Vertices[i].VertexColor = Color.FromRgb(blight, blight, blight);
                    // End of vertex shader
                }

                DrawScreenSpaceTriangleInterpolated(tri);
            }
        }

        public void DrawScreenSpaceTriangleInterpolated(PreparedTriangle triangle)
        {
            WriteableBitmap rt = _renderWindow.Framebuffer;

            if (triangle.GetScreenSpaceDirection() != VisibleTriangleDirection)
            {
                return;
            }

            var triangleArea = CalculateTriangleArea(
                triangle.Vertices[0].X, triangle.Vertices[0].Y, 
                triangle.Vertices[1].X, triangle.Vertices[1].Y,
                triangle.Vertices[2].X, triangle.Vertices[2].Y);

            var vertexX = new double[] { triangle.Vertices[0].X, triangle.Vertices[1].X, triangle.Vertices[2].X };
            var vertexY = new double[] { triangle.Vertices[0].Y, triangle.Vertices[1].Y, triangle.Vertices[2].Y };

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
                        var aArea = CalculateTriangleArea(
                            x, y,
                            triangle.Vertices[1].X, triangle.Vertices[1].Y,
                            triangle.Vertices[2].X, triangle.Vertices[2].Y);

                        var bArea = CalculateTriangleArea(
                            triangle.Vertices[0].X, triangle.Vertices[0].Y,
                            x, y,
                            triangle.Vertices[2].X, triangle.Vertices[2].Y);

                        var cArea = CalculateTriangleArea(
                            triangle.Vertices[0].X, triangle.Vertices[0].Y,
                            triangle.Vertices[1].X, triangle.Vertices[1].Y,
                            x, y);

                        var fa = Math.Min(aArea / triangleArea, 1.0);
                        var fb = Math.Min(bArea / triangleArea, 1.0);
                        var fc = Math.Min(cArea / triangleArea, 1.0);

                        var z = fa * triangle.Vertices[0].Z 
                            + fb * triangle.Vertices[1].Z 
                            + fc * triangle.Vertices[2].Z;

                        if (_zBuffer[x, y] < z)
                        {
                            continue;
                        }

                        _zBuffer[x, y] = z;

                        // Pixel Shader
                        Color outputColor = InterpolateColor(
                            fa, triangle.Vertices[0].VertexColor,
                            fb, triangle.Vertices[1].VertexColor,
                            fc, triangle.Vertices[2].VertexColor);
                        // End of pixel shader

                        rt.SetPixel(x, y, outputColor);
                    }
                }

                activeEdges.ForEach(t => t.X += t.XSlope);
            }
        }

        public Color InterpolateColor(double factorA, Color colorA, double factorB, Color colorB, double factorC, Color colorC)
        {
            var rChannel = factorA * colorA.R + factorB * colorB.R + factorC * colorC.R;
            var gChannel = factorA * colorA.G + factorB * colorB.G + factorC * colorC.G;
            var bChannel = factorA * colorA.B + factorB * colorB.B + factorC * colorC.B;

            rChannel = Math.Min(rChannel, 255.0);
            gChannel = Math.Min(gChannel, 255.0);
            bChannel = Math.Min(bChannel, 255.0);

            return Color.FromRgb((byte)rChannel, (byte)gChannel, (byte)bChannel);
        }

        public double CalculateTriangleArea(double ax, double ay, double bx, double by, 
            double cx, double cy)
        {
            var determinant = ax*by + ay*cx + bx*cy - ax*cy - ay*bx - by*cx;
            return Math.Abs(determinant * 0.5);
        }

        private class ScanEdge
        {
            public int MinimalY { get; set; }
            public int MaximalY { get; set; }
            public double X { get; set; }
            public double XSlope { get; set; }
        }
    }
}
