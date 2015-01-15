﻿using System;
using MathNet.Numerics.LinearAlgebra;

namespace SoftwareRenderer.Helpers
{
	public static class MatrixHelpers
	{
        /// <summary>
        /// Calculates look at vector
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lookAt"></param>
        /// <param name="upVector"></param>
        /// <returns></returns>
        public static Matrix<double> CalculateLookAt(Vector<double> position, Vector<double> lookAt, Vector<double> upVector)
        {
            var zAxis = (position - lookAt).Normalize(2);
            var xAxis = VectorHelpers.CrossProduct3D(upVector, zAxis).Normalize(2);
            var yAxis = VectorHelpers.CrossProduct3D(zAxis, xAxis).Normalize(2);

            return Matrix<double>.Build.DenseOfArray(new double[,] {
                {xAxis[0], yAxis[0], zAxis[0], position[0]	},
                {xAxis[1], yAxis[1], zAxis[1], position[1]	},
                {xAxis[2], yAxis[2], zAxis[2], position[2]	},
                {       0,        0,        0,          1	} 
			}).Inverse();
        }

        /// <summary>
        /// Calculates perspective projection matrix.
        /// </summary>
        /// <param name="near">Near plane</param>
        /// <param name="far">Far plane</param>
        /// <param name="fov">Field of view (in radians)</param>
        /// <param name="aspect">Aspect ratio</param>
        /// <returns></returns>
        public static Matrix<double> CalculatePerspectiveProjectionMatrix(double near, double far, double fov, double aspect)
        {
            var e = 1.0 / Math.Tan(fov / 2);

            return Matrix<double>.Build.DenseOfArray(new double[,] {
                {	e,			0,		                     0,  	    	         0   },
                {	0,	 e/aspect,		                     0,				         0   },
                {	0,			0,		-(far+near)/(far-near), -(2*far*near)/(far-near) },
                {	0,			0,		                    -1,                      0   }
            });
        }


	}
}