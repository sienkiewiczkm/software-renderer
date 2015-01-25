using System.Collections.Generic;
using SoftwareRenderer.Rendering;

namespace SoftwareRenderer.Helpers
{
    public static class MeshHelpers
    {
        public static WireframeMesh GetCubeWireframe(double a)
        {
            var h = a / 2;

            var model = new WireframeMesh
            {
                Edges = new List<Edge>
                {
                    new Edge(VectorHelpers.Create(-h, -h, -h), VectorHelpers.Create(-h, +h, -h)),
                    new Edge(VectorHelpers.Create(-h, +h, -h), VectorHelpers.Create(+h, +h, -h)),
                    new Edge(VectorHelpers.Create(+h, +h, -h), VectorHelpers.Create(+h, -h, -h)),
                    new Edge(VectorHelpers.Create(+h, -h, -h), VectorHelpers.Create(-h, -h, -h)),

                    new Edge(VectorHelpers.Create(-h, -h, +h), VectorHelpers.Create(-h, +h, +h)),
                    new Edge(VectorHelpers.Create(-h, +h, +h), VectorHelpers.Create(+h, +h, +h)),
                    new Edge(VectorHelpers.Create(+h, +h, +h), VectorHelpers.Create(+h, -h, +h)),
                    new Edge(VectorHelpers.Create(+h, -h, +h), VectorHelpers.Create(-h, -h, +h)),

                    new Edge(VectorHelpers.Create(-h, -h, -h), VectorHelpers.Create(-h, -h, +h)),
                    new Edge(VectorHelpers.Create(-h, +h, -h), VectorHelpers.Create(-h, +h, +h)),
                    new Edge(VectorHelpers.Create(+h, -h, -h), VectorHelpers.Create(+h, -h, +h)),
                    new Edge(VectorHelpers.Create(+h, +h, -h), VectorHelpers.Create(+h, +h, +h))
                }
            };

            return model;
        }

        public static Mesh GetCube(double a)
        {
            var h = a/2;

            var model = new Mesh
            {
                Vertices = new List<Vertex>
                {
                    // Top
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, -h), Normal = VectorHelpers.Create(+0, +1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, -h), Normal = VectorHelpers.Create(+0, +1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, +h), Normal = VectorHelpers.Create(+0, +1, +0)},
                    
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, +h), Normal = VectorHelpers.Create(+0, +1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, -h), Normal = VectorHelpers.Create(+0, +1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, +h), Normal = VectorHelpers.Create(+0, +1, +0)},

                    // Bottom
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, +h), Normal = VectorHelpers.Create(+0, -1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, -h, +h), Normal = VectorHelpers.Create(+0, -1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, -h), Normal = VectorHelpers.Create(+0, -1, +0)},
                    
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, -h), Normal = VectorHelpers.Create(+0, +1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, -h, +h), Normal = VectorHelpers.Create(+0, +1, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, -h, -h), Normal = VectorHelpers.Create(+0, +1, +0)},

                    
                    // Front
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, +h), Normal = VectorHelpers.Create(+0, +0, +1)},
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, +h), Normal = VectorHelpers.Create(+0, +0, +1)},
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, +h), Normal = VectorHelpers.Create(+0, +0, +1)},
                    
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, +h), Normal = VectorHelpers.Create(+0, +0, +1)},
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, +h), Normal = VectorHelpers.Create(+0, +0, +1)},
                    new Vertex() {Position = VectorHelpers.Create(+h, -h, +h), Normal = VectorHelpers.Create(+0, +0, +1)},
                    
                    // Back
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, -h), Normal = VectorHelpers.Create(+0, +0, -1)},
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, -h), Normal = VectorHelpers.Create(+0, +0, -1)},
                    new Vertex() {Position = VectorHelpers.Create(+h, -h, -h), Normal = VectorHelpers.Create(+0, +0, -1)},

                    new Vertex() {Position = VectorHelpers.Create(+h, -h, -h), Normal = VectorHelpers.Create(+0, +0, -1)},
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, -h), Normal = VectorHelpers.Create(+0, +0, -1)},
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, -h), Normal = VectorHelpers.Create(+0, +0, -1)},

                    // Left
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, -h), Normal = VectorHelpers.Create(-1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, +h), Normal = VectorHelpers.Create(-1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, -h), Normal = VectorHelpers.Create(-1, +0, +0)},
                    
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, -h), Normal = VectorHelpers.Create(-1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(-h, +h, +h), Normal = VectorHelpers.Create(-1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(-h, -h, +h), Normal = VectorHelpers.Create(-1, +0, +0)},

                    // Right
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, +h), Normal = VectorHelpers.Create(+1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, -h), Normal = VectorHelpers.Create(+1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, -h, +h), Normal = VectorHelpers.Create(+1, +0, +0)},

                    new Vertex() {Position = VectorHelpers.Create(+h, -h, +h), Normal = VectorHelpers.Create(+1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, +h, -h), Normal = VectorHelpers.Create(+1, +0, +0)},
                    new Vertex() {Position = VectorHelpers.Create(+h, -h, -h), Normal = VectorHelpers.Create(+1, +0, +0)},
                }
            };

            return model;
        }   

    }
}
