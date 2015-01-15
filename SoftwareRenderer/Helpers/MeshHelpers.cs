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

    }
}
