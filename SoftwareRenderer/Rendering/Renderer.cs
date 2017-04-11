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

        public double[] TextureCoordinates { get; protected set; }
        public double U { get { return TextureCoordinates[0]; } set { TextureCoordinates[0] = value; } }
        public double V { get { return TextureCoordinates[1]; } set { TextureCoordinates[1] = value; } }

        public Color VertexColor { get; set; }

        public PreparedVertex()
        {
            Coordinates = new double[3];
            TextureCoordinates = new double[2];
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

        public void SetTextureCoordinates(Vector<double> vector)
        {
            if (vector.Count < 2)
            {
                throw new ArgumentException("Vector must have at least 2 dimmensions.");
            }

            U = vector[0];
            V = vector[1];
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

    public class Material
    {
        public Color AmbientColor { get; set; }
        public Color DiffuseColor { get; set; }
        public Color SpecularColor { get; set; }
        public double ShineFactor { get; set; }

        public WriteableBitmap DiffuseTexture { get; set; }
    }

    public class PointLight
    {
        public Color Color { get; set; }
        public Vector<double> Position { get; set; }

        public double AttenuationConstantFactor { get; set; }
        public double AttenuationLinearFactor { get; set; }
        public double AttenuationQuadraticFactor { get; set; }

        public PointLight()
        {
            AttenuationConstantFactor = 1.0;
            AttenuationLinearFactor = 0.0;
            AttenuationQuadraticFactor = 0.0;
        }

        public double GetAttenuationFactor(double distance)
        {
            return 1 / (AttenuationConstantFactor
                + AttenuationLinearFactor * distance
                + AttenuationQuadraticFactor * distance * distance);
        }
    }

    public class Renderer
    {
        private readonly IRenderWindow _renderWindow;
        private double[,] _zBuffer;

        public ScreenSpaceTriangleDirection VisibleTriangleDirection { get; set; }
        public bool TexturingEnabled { get; set; }
        public Material Material { get; set; }

        public List<PointLight> Lights { get; set; }

        public Vector<double> WorldPositionEye { get; set; }

        public bool AmbientLightingEnabled { get; set; }
        public bool DiffuseLightingEnabled { get; set; }
        public bool SpecularLightingEnabled { get; set; }

        public Renderer(IRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;

            VisibleTriangleDirection = ScreenSpaceTriangleDirection.Clockwise;

            //Texture = new WriteableBitmap(new BitmapImage(new Uri("Data/Textures/darkstone.png", UriKind.Relative)));
            TexturingEnabled = true;
            AmbientLightingEnabled = true;
            DiffuseLightingEnabled = true;
            SpecularLightingEnabled = true;

            Lights = new List<PointLight>();

            _zBuffer = new double[_renderWindow.Framebuffer.PixelWidth, _renderWindow.Framebuffer.PixelHeight];
        }

        public void InitializeBuffers()
        {
            var width = _renderWindow.Framebuffer.PixelWidth;
            var height = _renderWindow.Framebuffer.PixelHeight;
        }

        public void BeginFrame()
        {   
            var rt = _renderWindow.Framebuffer;

            rt.Lock();
            rt.Clear(Colors.Black);

            var hw = rt.PixelWidth*0.5;
            var hh = rt.PixelHeight*0.5;

            for (int y = 0; y < rt.PixelHeight; ++y)
            {
                for (int x = 0; x < rt.PixelWidth; ++x)
                {
                    _zBuffer[x, y] = Double.MinValue;
                }
            }
        }

        public void EndFrame()
        {
            _renderWindow.Framebuffer.Unlock();
        }

        public void RenderObjMesh(ObjData meshToRender, Matrix<double> transformation, Matrix<double> normalTransformation)
        {
            if (Material == null)
            {
                Material = new Material();
            }

            if (TexturingEnabled)
            {
                if (Material.DiffuseTexture != null)
                {
                    Material.DiffuseTexture.Lock();
                }
            }

            var hw = _renderWindow.Framebuffer.PixelWidth * 0.5;
            var hh = _renderWindow.Framebuffer.PixelHeight * 0.5;

            foreach (var triangle in meshToRender.Triangles)
            {
                var tri = new PreparedTriangle();

                for (int i = 0; i < 3; ++i)
                {
                    Vector<double> modelSpacePosition = meshToRender.Vertices[triangle.Vertices[i]];
                    Vector<double> modelSpaceNormal = meshToRender.Normals[triangle.Normals[i]];

                    Vector<double> screenSpacePosition = (transformation * modelSpacePosition.ExtendVector())
                        .ToCartesian()
                        .Add(VectorHelpers.Create(1.0, 1.0, 0.0))
                        .PointwiseMultiply(VectorHelpers.Create(hw, hh, 1.0));

                    Vector<double> worldSpacePosition = (normalTransformation * modelSpacePosition.ExtendVector())
                        .ToCartesian();
                    Vector<double> worldSpaceNormal = (normalTransformation * modelSpaceNormal.ExtendVector(0.0))
                        .DiscardLastCoordinate().Normalize(2);

                    // Vertex shader
                    tri.Vertices[i].SetCoordinates(screenSpacePosition);
                    tri.Vertices[i].SetTextureCoordinates(meshToRender.TexCoords[triangle.TexCoords[i]]);

                    var vR = 0;
                    var vG = 0;
                    var vB = 0;

                    if (AmbientLightingEnabled)
                    {
                        vR += Material.AmbientColor.R;
                        vG += Material.AmbientColor.G;
                        vB += Material.AmbientColor.B;
                    }

                    if (DiffuseLightingEnabled || SpecularLightingEnabled)
                    {
                        foreach (var light in Lights)
                        {
                            var lightDirection = (light.Position - worldSpacePosition).Normalize(2);

                            if (DiffuseLightingEnabled)
                            {
                                var diffuseLightFactor = Math.Max(lightDirection.DotProduct(worldSpaceNormal), 0);
                                var fullSaturationDiffuseColor = MultiplyColors(light.Color, Material.DiffuseColor);

                                vR += (byte)(fullSaturationDiffuseColor.R * diffuseLightFactor);
                                vG += (byte)(fullSaturationDiffuseColor.G * diffuseLightFactor);
                                vB += (byte)(fullSaturationDiffuseColor.B * diffuseLightFactor);
                            }

                            if (SpecularLightingEnabled)
                            {
                                var eyeDirection = (WorldPositionEye - worldSpacePosition).Normalize(2);
                                var reflected = (2 * worldSpaceNormal.DotProduct(lightDirection) * worldSpaceNormal - lightDirection).Normalize(2);
                                var specularLightFactor = Math.Pow(Math.Max(eyeDirection.DotProduct(reflected), 0), Material.ShineFactor);
                                var fullSaturationSpecularColor = MultiplyColors(light.Color, Material.SpecularColor);

                                vR += (byte)(fullSaturationSpecularColor.R * specularLightFactor);
                                vG += (byte)(fullSaturationSpecularColor.G * specularLightFactor);
                                vB += (byte)(fullSaturationSpecularColor.B * specularLightFactor);
                            }
                        }
                    }

                    tri.Vertices[i].VertexColor = Color.FromRgb(
                        (byte)Math.Min(vR, 255),
                        (byte)Math.Min(vG, 255), 
                        (byte)Math.Min(vB, 255));
                    // End of vertex shader
                }

                DrawScreenSpaceTriangleInterpolated(tri);
            }

            if (TexturingEnabled && Material.DiffuseTexture != null)
            {
                Material.DiffuseTexture.Unlock();
            }
        }

        protected void DrawScreenSpaceTriangleInterpolated(PreparedTriangle triangle)
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
                //int startX = (int)higherx;
                //int endX = (int)lowerx;

                if (minY == maxY)
                {
                    continue;
                }

                var scanEdge = new ScanEdge();
                scanEdge.MinimalY = (int)Math.Ceiling(highery);
                scanEdge.MaximalY = (int)Math.Ceiling(lowery);
                scanEdge.X = higherx;
                scanEdge.XSlope = (lowerx - higherx) / ((double)(lowery - highery));

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
                    int startX = (int)Math.Ceiling(Math.Max(activeEdges[i].X, 0));
                    int endX = (int)Math.Floor(Math.Min(activeEdges[i + 1].X, rt.PixelWidth-1));

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

                        if (fa + fb + fc > 1.0)
                        {
                            int lowestAreaIndex = 0;
                            double lowestArea = fa;

                            if (lowestArea > fb)
                            {
                                lowestAreaIndex = 1;
                                lowestArea = fb;
                            }

                            if (lowestArea > fc)
                            {
                                lowestAreaIndex = 2;
                                lowestArea = fc;
                            }

                            switch (lowestAreaIndex)
                            {
                                case 0:
                                    fa = 1.0 - fb - fc;
                                    break;
                                case 1:
                                    fb = 1.0 - fa - fc;
                                    break;
                                case 2:
                                    fc = 1.0 - fa - fb;
                                    break;
                            }
                        }

                        var z = fa * triangle.Vertices[0].Z 
                            + fb * triangle.Vertices[1].Z 
                            + fc * triangle.Vertices[2].Z;

                        if (z <= 1.0)
                        {
                            continue;
                        }

                        if (_zBuffer[x, y] > z)
                        {
                            continue;
                        }

                        _zBuffer[x, y] = z;

                        // Pixel Shader
                        Color outputColor = InterpolateColor(
                            fa, triangle.Vertices[0].VertexColor,
                            fb, triangle.Vertices[1].VertexColor,
                            fc, triangle.Vertices[2].VertexColor);

                        if (TexturingEnabled && Material.DiffuseTexture != null)
                        {
                            var u = fa * triangle.Vertices[0].U + fb * triangle.Vertices[1].U + fc * triangle.Vertices[2].U;
                            var v = fa * triangle.Vertices[0].V + fb * triangle.Vertices[1].V + fc * triangle.Vertices[2].V;

                            u = Math.Min(Math.Max(u, 0.0), 1.0);
                            v = Math.Min(Math.Max(v, 0.0), 1.0);

                            Color texColor = Material.DiffuseTexture.GetPixel((int)(u * Material.DiffuseTexture.PixelWidth), 
                                (int)(v * Material.DiffuseTexture.PixelHeight));
                            outputColor = MultiplyColors(outputColor, texColor);
                        }
                        // End of pixel shader

                        rt.SetPixel(x, y, outputColor);
                    }
                }

                activeEdges.ForEach(t => t.X += t.XSlope);
            }
        }

        protected Color MultiplyColors(Color a, Color b)
        {
            var aR = a.R / 255.0;
            var aG = a.G / 255.0;
            var aB = a.B / 255.0;

            var bR = b.R / 255.0;
            var bG = b.G / 255.0;
            var bB = b.B / 255.0;

            var fR = (byte)(aR * bR * 255);
            var fG = (byte)(aG * bG * 255);
            var fB = (byte)(aB * bB * 255);

            return Color.FromRgb(fR, fG, fB);

        }

        protected Color InterpolateColor(double factorA, Color colorA, double factorB, Color colorB, double factorC, Color colorC)
        {
            var rChannel = factorA * colorA.R + factorB * colorB.R + factorC * colorC.R;
            var gChannel = factorA * colorA.G + factorB * colorB.G + factorC * colorC.G;
            var bChannel = factorA * colorA.B + factorB * colorB.B + factorC * colorC.B;

            rChannel = Math.Min(rChannel, 255.0);
            gChannel = Math.Min(gChannel, 255.0);
            bChannel = Math.Min(bChannel, 255.0);

            return Color.FromRgb((byte)rChannel, (byte)gChannel, (byte)bChannel);
        }

        protected double CalculateTriangleArea(double ax, double ay, double bx, double by, 
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
