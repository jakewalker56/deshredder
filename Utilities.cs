using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PetraAndJakesDeShredder
{
    class Utilities
    {
        public static readonly int Threshold = 4000;
        public static bool SameColor(Color lhs, Color rhs)
        {
            if (Math.Pow((lhs.R - rhs.R), 2) > Threshold)
            {
                return false;
            }
            if (Math.Pow((lhs.G - rhs.G), 2) > Threshold)
            {
                return false;
            }
            if (Math.Pow((lhs.B - rhs.B), 2) > Threshold)
            {
                return false;
            }
            return true;
        }
    }
}
