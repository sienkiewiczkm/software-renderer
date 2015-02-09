using System;
using MathNet.Numerics.LinearAlgebra;

namespace SoftwareRenderer.Helpers
{
    public static class VectorHelpers
    {
        public static Vector<double> Create(double x, double y)
        {
            return Vector<double>.Build.DenseOfArray(new[] { x, y });
        }

        public static Vector<double> Create(double x, double y, double z)
        {
            return Vector<double>.Build.DenseOfArray(new[] {x, y, z});
        }

        public static Vector<double> Create(double x, double y, double z, double w)
        {
            return Vector<double>.Build.DenseOfArray(new[] {x, y, z, w});
        }

        public static Vector<double> ToCartesian(this Vector<double> vector)
        {
            if (vector.Count == 3)
            {
                return vector.Clone();
            }

            if (vector.Count == 4)
            {
                return Vector<double>.Build.DenseOfArray(new[]
                {
                    vector[0]/vector[3],
                    vector[1]/vector[3],
                    vector[2]/vector[3]
                });
            }

            throw new InvalidOperationException("Provided vector is neither of size 3 nor 4");
        }

        public static Vector<double> CrossProduct3D(Vector<double> vector1, Vector<double> vector2)
        {
            if (vector1.Count != 3 || vector2.Count != 3)
            {
                throw new InvalidOperationException("Provided vectors are not 3-dimmensional.");
            }

            return Vector<double>.Build.DenseOfArray(new[]
            {
                vector1[1] * vector2[2] - vector1[2]* vector2[1], 
                vector1[2] * vector2[0] - vector1[0]* vector2[2], 
                vector1[0] * vector2[1] - vector1[1]* vector2[0]
            });
        }

        public static Vector<double> ExtendVector(this Vector<double> vector, double extension = 1.0)
        {
            var oneDimmensionLarger = Vector<double>.Build.Dense(vector.Count + 1);
            for (var i = 0; i < vector.Count; ++i)
            {
                oneDimmensionLarger[i] = vector[i];
            }
            oneDimmensionLarger[vector.Count] = extension;
            return oneDimmensionLarger;
        }

        public static Vector<double> DiscardLastCoordinate(this Vector<double> vector)
        {
            var oneDimmensionSmaller = Vector<double>.Build.Dense(vector.Count - 1);
            for (var i = 0; i < vector.Count-1; ++i)
            {
                oneDimmensionSmaller[i] = vector[i];
            }
            return oneDimmensionSmaller;
        }
    }
}
