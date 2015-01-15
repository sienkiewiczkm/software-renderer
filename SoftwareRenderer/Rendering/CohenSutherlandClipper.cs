using System;
using MathNet.Numerics.LinearAlgebra;
using SoftwareRenderer.Helpers;

namespace SoftwareRenderer.Rendering
{
    public class CohenSutherlandClipper
    {
        public double XCoordinateMinimum { get; set; }
        public double XCoordinateMaximum { get; set; }
        public double YCoordinateMinimum { get; set; }
        public double YCoordinateMaximum { get; set; }
        public double ZCoordinateMinimum { get; set; }
        public double ZCoordinateMaximum { get; set; }

        public CohenSutherlandClipper()
        {
            XCoordinateMinimum = -1.0;
            XCoordinateMaximum = +1.0;
            YCoordinateMinimum = -1.0;
            YCoordinateMaximum = +1.0;
            ZCoordinateMinimum = -1.0;
            ZCoordinateMaximum = +1.0;
        }

        private enum SectorCode
        {
            UnderXMinimum = 0,
            AboveXMaximum = 1,
            UnderYMinimum = 2,
            AboveYMaximum = 3,
            UnderZMinimum = 4,
            AboveZMaximum = 5,
        }

        public int GetVectorCode(Vector<double> vector)
        {
            if (vector.Count != 3)
            {
                throw new InvalidOperationException(
                    "Three-dimmensional vector is required to calculate Cohen-Sutheland code.");
            }

            int code = 0;

            if (vector[0] < XCoordinateMinimum)
            {
                code |= (1 << (int)SectorCode.UnderXMinimum);
            }

            if (vector[0] > XCoordinateMaximum)
            {
                code |= (1 << (int)SectorCode.AboveXMaximum);
            }

            if (vector[1] < YCoordinateMinimum)
            {
                code |= (1 << (int)SectorCode.UnderYMinimum);
            }

            if (vector[1] > YCoordinateMaximum)
            {
                code |= (1 << (int)SectorCode.AboveYMaximum);
            }

            if (vector[2] < ZCoordinateMinimum)
            {
                code |= (1 << (int)SectorCode.UnderZMinimum);
            }

            if (vector[2] > ZCoordinateMaximum)
            {
                code |= (1 << (int)SectorCode.AboveZMaximum);
            }

            return code;
        }

        public bool Clip(Vector<double> a, Vector<double> b, 
            out Vector<double> aClipped, out Vector<double> bClipped)
        {
            var abSwapped = false;

            aClipped = null;
            bClipped = null;

            do
            {
                var aCode = GetVectorCode(a);
                var bCode = GetVectorCode(b);

                if ((aCode | bCode) == 0)
                {
                    if (abSwapped)
                    {
                        MiscHelpers.Swap(ref a, ref b);
                    }

                    aClipped = a.Clone();
                    bClipped = b.Clone();

                    return true;
                }

                if ((aCode & bCode) != 0)
                {
                    return false;
                }

                if (aCode == 0)
                {
                    MiscHelpers.Swap(ref a, ref b);
                    MiscHelpers.Swap(ref aCode, ref bCode);
                    abSwapped = !abSwapped;
                }

                var direction = b - a;

                if ((aCode & (1 << (int)SectorCode.UnderXMinimum)) != 0)
                {
                    var t = -(a[0] + 1)/direction[0];
                    a = a + t*direction;
                }
                else if ((aCode & (1 << (int)SectorCode.AboveXMaximum)) != 0)
                {
                    var t = -(a[0] - 1) / direction[0];
                    a = a + t * direction;
                }
                else if ((aCode & (1 << (int)SectorCode.UnderYMinimum)) != 0)
                {
                    var t = -(a[1] + 1) / direction[1];
                    a = a + t * direction;
                }
                else if ((aCode & (1 << (int)SectorCode.AboveYMaximum)) != 0)
                {
                    var t = -(a[1] - 1) / direction[1];
                    a = a + t * direction;
                }
                else if ((aCode & (1 << (int)SectorCode.UnderZMinimum)) != 0)
                {
                    var t = -(a[2] + 1) / direction[2];
                    a = a + t * direction;
                }
                else if ((aCode & (1 << (int)SectorCode.AboveZMaximum)) != 0)
                {
                    var t = -(a[2] - 1) / direction[2];
                    a = a + t * direction;
                }

            } 
            while (true);
        }
    }
}
