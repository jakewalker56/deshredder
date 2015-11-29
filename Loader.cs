using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PetraAndJakesDeShredder
{
    class Loader
    {
        public static readonly string ImageDirectory = @"C:\Users\Jake\github\Projects\Deshredder\";
        public static readonly string InputFilename = @"Image";
        public static readonly string DeconstructedFileNamePrefix = @"DeconstructedImage_";
        public static readonly string AnalyzedFileNamePrefix = @"AnalyzedImage_"; 
        public static readonly string SynthesizedFileNamePrefix = @"SynthesizedImage_";

        public static Bitmap LoadInput()
        {
            Bitmap b = new Bitmap(string.Format("{0}{1}.tif", ImageDirectory, InputFilename));
            return b;
        }
        public static void Output(Bitmap b, string filename)
        {
            b.Save(filename);
        }
        public static void OutputDeconstrucedBitmaps(List<Bitmap> DeconstructedImages)
        { 
        
        }
        public static void PrintLineSet(List<Line> lineset)
        {
            foreach (Line l in lineset)
            {
                Console.WriteLine(string.Format("\t\tY: {0} Width: {1}", l.y, l.Width));
            }
        }
        public static void OutputSynthesizedBitmaps(List<AnalyzedImage> SynthesizedImages)
        {
            int OutputIndex = 1;
            foreach (AnalyzedImage image in SynthesizedImages)
            {
                //image.bitmap.Save(String.Format("{0}{1}{2}.tif", ImageDirectory, SynthesizedFileNamePrefix, image.Name));
                image.BlackAndWhiteBitmap.Save(String.Format("{0}{1}{2}_BlackAndWhite.tif", ImageDirectory, SynthesizedFileNamePrefix, image.Name));
                OutputIndex++;
            }
        }
        public static void OutputAnalyzedBitmaps(List<AnalyzedImage> AnalyzedImages)
        {
            int OutputIndex = 1;
            foreach (AnalyzedImage image in AnalyzedImages)
            {
                //image.bitmap.Save(String.Format("{0}{1}{2}.tif", ImageDirectory, AnalyzedFileNamePrefix, OutputIndex));
                image.BlackAndWhiteBitmap.Save(String.Format("{0}{1}{2}_BlackAndWhite.tif", ImageDirectory, AnalyzedFileNamePrefix, OutputIndex));
                OutputIndex++;
            }
        }
        public static List<Bitmap> LoadDeconstructedBitmaps()
        {
            int InputIndex = 1;
            List<Bitmap> result = new List<Bitmap>();
            while (System.IO.File.Exists(String.Format("{0}{1}{2}.tif", ImageDirectory, DeconstructedFileNamePrefix, InputIndex)))
            {
                Console.WriteLine(String.Format("Loaded {0}{1}{2}.tif", ImageDirectory, DeconstructedFileNamePrefix, InputIndex));
                Bitmap b = new Bitmap(String.Format("{0}{1}{2}.tif", ImageDirectory, DeconstructedFileNamePrefix, InputIndex));
                result.Add(b);
                InputIndex++;
            }
            Console.WriteLine("finished loading deconstructed images");
            return result;
        }

    }
}
