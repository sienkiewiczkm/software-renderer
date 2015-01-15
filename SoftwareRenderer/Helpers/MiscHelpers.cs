using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRenderer.Helpers
{
    public static class MiscHelpers
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            var temporary = a;
            a = b;
            b = temporary;
        }
    }
}
