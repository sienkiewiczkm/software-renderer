using MathNet.Numerics.LinearAlgebra;
using SoftwareRenderer.Helpers;

namespace SoftwareRenderer.Rendering
{
    public struct Edge
    {
        public Vector<double> From;
        public Vector<double> To;

        public Edge(Vector<double> from, Vector<double> to)
        {
            From = from;
            To = to;
        }
    }

    public struct Vertex
    {
        public Vector<double> Position;
        public Vector<double> Normal;

        public void TransformPosition(Matrix<double> matrix4X4)
        {
            Position = (matrix4X4*Position.ExtendVector()).ToCartesian();
        }
    }

    public struct Polygon
    {
        public Vertex[] Vertices;

        public bool IsTriangle
        {
            get { return Vertices != null && Vertices.Length == 3; }
        }
    }
}
