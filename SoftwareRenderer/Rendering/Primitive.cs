using MathNet.Numerics.LinearAlgebra;

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

    public struct Triangle
    {
    }
}
