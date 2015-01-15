using System;
using MathNet.Numerics.LinearAlgebra;
using SoftwareRenderer.Helpers;

namespace SoftwareRenderer.Rendering
{
    public class Camera
    {
        public Vector<double> Position { get; set; }
        public Vector<double> LookAt { get; set; }
        public Vector<double> UpVector { get; set; }

        public double FieldOfView { get; set; }
        public double AspectRatio { get; set; }
        public double NearPlane { get; set; }
        public double FarPlane { get; set; }

        public Camera()
        {
            FieldOfView = Math.PI/4;
            AspectRatio = 1;
            NearPlane = 1;
            FarPlane = 100;

            UpVector = VectorHelpers.Create(0, 1, 0);
            Position = VectorHelpers.Create(0, 0, 0);
            LookAt = VectorHelpers.Create(0, 0, -1);
        }

        public Matrix<double> GetViewMatrix()
        {
            return MatrixHelpers.CalculateLookAt(Position, LookAt, UpVector);
        }

        public Matrix<double> GetProjectionMatrix()
        {
            return MatrixHelpers.CalculatePerspectiveProjectionMatrix(NearPlane, FarPlane, 
                FieldOfView, AspectRatio);
        }
    }
}
