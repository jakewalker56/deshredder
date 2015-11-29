using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PetraAndJakesDeShredder
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap Input = Loader.LoadInput();
            Deconstructor.Deconstruct(Input);
            List<Bitmap> DeconstrucedBitmaps = Loader.LoadDeconstructedBitmaps();
            List<Color> BackgroundColors = new List<Color>();
            List<Color> PaperColors = new List<Color>(); 
            BackgroundColors.Add(Color.FromArgb(255, 0, 135));
            PaperColors.Add(Color.FromArgb(255, 71, 123)); 
            PaperColors.Add(Color.FromArgb(243, 227, 71));
            PaperColors.Add(Color.FromArgb(252, 198, 191));
            BackgroundColors.Add(Color.FromArgb(214, 11, 12));
            PaperColors.Add(Color.FromArgb(215, 116, 52));
            PaperColors.Add(Color.FromArgb(244, 224, 1));
            PaperColors.Add(Color.FromArgb(232, 157, 123));
            List<AnalyzedImage> AnalyzedImages = Analyzer.Analyze(DeconstrucedBitmaps, BackgroundColors, PaperColors);
            Synthesizer.Synthesize(AnalyzedImages.ToArray());
        }
    }
}
