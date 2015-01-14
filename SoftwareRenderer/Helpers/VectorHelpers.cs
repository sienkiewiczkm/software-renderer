using MathNet.Numerics.LinearAlgebra;

namespace SoftwareRenderer.Helpers
{
	public static class VectorHelpers
	{
		public static Vector<double> Create(double x, double y, double z)
		{
			return Vector<double>.Build.DenseOfArray(new[] { x, y, z });
		}

		public static Vector<double> Create(double x, double y, double z, double w)
		{
			return Vector<double>.Build.DenseOfArray(new[] {x, y, z, w});
		}
	}
}
