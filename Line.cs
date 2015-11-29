using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PetraAndJakesDeShredder
{
    class Line
    {
        public int Width; //how wide the line is
        public int y; //the y coordinate for the center of the line
        public int MaxY; //the maximum possible value for y.  need this in order to reverse the lines
        public Line(int LineWidth, int LineY, int MaximumY)
        {
            Width = LineWidth;
            y = LineY;
            MaxY = MaximumY;
        }
    }
}
