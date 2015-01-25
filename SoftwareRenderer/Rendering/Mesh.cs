using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MathNet.Numerics.LinearAlgebra;

namespace SoftwareRenderer.Rendering
{
    public class Material
    {
        public Color DiffuseColor { get; set; }
    }

    public class Mesh
    {
        public List<Vertex> Vertices { get; set; } 
        public Material Material { get; set; }
        public Matrix<double> ModelMatrix;

        public Mesh()
        {
        }
    }
}
