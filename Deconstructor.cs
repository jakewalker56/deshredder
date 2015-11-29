using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PetraAndJakesDeShredder
{
    class Deconstructor
    {
        public static void Deconstruct(Bitmap image)
        {
            /*
             * assumptions we might make:
             * 
             * Background is uniform color
             * Pieces are different color (we can have this as input if we want... there's only 6 inputs)
             * 
            */
            List<Bitmap> result = new List<Bitmap>();
            result.Add(image);

            Loader.OutputDeconstrucedBitmaps(result);
        }
    }
}
