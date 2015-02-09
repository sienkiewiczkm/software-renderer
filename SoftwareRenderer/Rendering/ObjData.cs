using MathNet.Numerics.LinearAlgebra;
using SoftwareRenderer.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRenderer.Rendering
{
    public class IndexedTriangle
    {
        public int[] Vertices { get; private set; }
        public int[] TexCoords { get; private set; }
        public int[] Normals { get; private set; }

        public IndexedTriangle()
        {
            Vertices = new int[3];
            TexCoords = new int[3];
            Normals = new int[3];
        }
    }

    public class ObjData
    {
        public List<Vector<double>> Vertices { get; set; }
        public List<Vector<double>> TexCoords { get; set; }
        public List<Vector<double>> Normals { get; set; }
        public List<IndexedTriangle> Triangles { get; set; }

        public ObjData()
        {
            Vertices = new List<Vector<double>>();
            TexCoords = new List<Vector<double>>();
            Normals = new List<Vector<double>>();
            Triangles = new List<IndexedTriangle>();
        }

        public static ObjData LoadFromFile(string filePath)
        {
            using (var streamReader = new StreamReader(filePath))
            {
                var obj = new ObjData();

                string line = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line[0] == '#')
                    {
                        continue;
                    }

                    var tokens = line.Split(' ');

                    switch (tokens[0])
                    {
                        case "v":
                        {
                            double x = Double.Parse(tokens[1], CultureInfo.InvariantCulture);
                            double y = Double.Parse(tokens[2], CultureInfo.InvariantCulture);
                            double z = Double.Parse(tokens[3], CultureInfo.InvariantCulture);

                            obj.Vertices.Add(VectorHelpers.Create(x, y, z));
                            break;
                        }
                        case "vt":
                        {
                            double u = Double.Parse(tokens[1], CultureInfo.InvariantCulture);
                            double v = Double.Parse(tokens[2], CultureInfo.InvariantCulture);
                            obj.TexCoords.Add(VectorHelpers.Create(u, v));
                            break;
                        }
                        case "vn":
                        {
                            double x = Double.Parse(tokens[1], CultureInfo.InvariantCulture);
                            double y = Double.Parse(tokens[2], CultureInfo.InvariantCulture);
                            double z = Double.Parse(tokens[3], CultureInfo.InvariantCulture);

                            obj.Normals.Add(VectorHelpers.Create(x, y, z));
                            break;
                        }
                        case "f":
                        {
                            var triangle = new IndexedTriangle();
                            for (int i = 0; i < 3; ++i)
                            {
                                var vertexTokens = tokens[i + 1].Split('/');
                                triangle.Vertices[i] = Int32.Parse(vertexTokens[0]) - 1;
                                triangle.TexCoords[i] = Int32.Parse(vertexTokens[1]) - 1;
                                triangle.Normals[i] = Int32.Parse(vertexTokens[2]) - 1;
                            }
                            obj.Triangles.Add(triangle);
                            break;
                        }
                    }
                }

                return obj;
            }
        }
    }
}
