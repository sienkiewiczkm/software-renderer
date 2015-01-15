using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace SoftwareRenderer.Rendering
{
    public class WireframeMesh
    {
        public List<Edge> Edges { get; set; }
        public Matrix<double> ModelMatrix { get; set; }

        public WireframeMesh()
        {
            ModelMatrix = Matrix<double>.Build.DenseIdentity(4);
        }
    }

}
