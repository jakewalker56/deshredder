using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PetraAndJakesDeShredder
{
    class Point
    {
        public int x;
        public int y;
        public Point(int i, int j)
        {
            x = i;
            y = j;
        }
        public int Value(int i, int j) //distance approximation
        {
            int resultx = i - x;
            int resulty = j - y;
            resultx = (resultx > 0) ? resultx : -resultx;
            resulty = (resulty > 0) ? resulty : -resulty;
            return resultx + resulty;
        }
    }
}
