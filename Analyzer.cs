using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
     
namespace PetraAndJakesDeShredder
{
    class Analyzer
    {
    
        public static bool SmoothLines = true;
        
        public static readonly int PIXEL_OFFSET_ARRAY_SIZE = 3;
        public static int[] PixelOffsetArray;
        public static void FindLines(AnalyzedImage bAnalyzedImage)
        {
            //scan down until we find a line
            //look at the edge pixel, and pixels PixelOffset away from the edge
            //keep going until we find the end of the line
            //create the line, add to Line list, move to next pixel

            PixelOffsetArray = new int[PIXEL_OFFSET_ARRAY_SIZE];
            PixelOffsetArray[0] = 0;
            PixelOffsetArray[1] = 3;
            PixelOffsetArray[2] = -3;
           
            int ExpectedEdgeLocation = 0; // = (bAnalyzedImage.TopRight.x + bAnalyzedImage.BottomRight.x) / 2;
            int ConsecutiveLineValues = 0;
            for (int j = bAnalyzedImage.TopRight.y; j < bAnalyzedImage.BottomRight.y; j++)
            {
                bool MiddleOfLine = false;
                for (int i = 0; i < PIXEL_OFFSET_ARRAY_SIZE; i++)
                {
                    if (!MiddleOfLine)
                    {
                        //find ExpectedEdgeLocation (if not already tracking a line)
                        for (int k = 0; k < 30; k++)
                        {
                            if (bAnalyzedImage.IsPaper(bAnalyzedImage.BlackAndWhiteBitmap.Width - k - 1, j) || bAnalyzedImage.IsLine(bAnalyzedImage.BlackAndWhiteBitmap.Width - k - 1, j))
                            {
                                ExpectedEdgeLocation = bAnalyzedImage.BlackAndWhiteBitmap.Width - k - 1;
                                break;
                            }
                        }
                    }
                    if (bAnalyzedImage.InBitmapRangeX(ExpectedEdgeLocation + PixelOffsetArray[i]))
                    {
                        if (bAnalyzedImage.IsLine(ExpectedEdgeLocation + PixelOffsetArray[i], j))
                        {
                            if (!MiddleOfLine)
                            {
                                MiddleOfLine = true;
                                ConsecutiveLineValues++;
                            }
                        }
                    }
                }
                if (!MiddleOfLine || j == bAnalyzedImage.BottomRight.y - 1)
                {
                    //we've reached the end of the line (or end of scan).  add to the list
                    if (ConsecutiveLineValues > 3) //ignore spurious small lines
                    {
                        Line l = new Line(ConsecutiveLineValues, j - (ConsecutiveLineValues / 2), bAnalyzedImage.bitmap.Height - 1);
                        bAnalyzedImage.RightHandSideLineList.Add(l);
                    }
                    ConsecutiveLineValues = 0;
                }
            }
            ExpectedEdgeLocation = (bAnalyzedImage.TopLeft.x + bAnalyzedImage.BottomLeft.x) / 2;
            ConsecutiveLineValues = 0;
            for (int j = bAnalyzedImage.TopLeft.y; j < bAnalyzedImage.BottomLeft.y; j++)
            {
                bool MiddleOfLine = false;
                for (int i = 0; i < PIXEL_OFFSET_ARRAY_SIZE; i++)
                {
                    if (!MiddleOfLine)
                    {
                        //find ExpectedEdgeLocation (if not already tracking a line)
                        for (int k = 0; k < 30; k++)
                        {
                            if (bAnalyzedImage.IsPaper(k, j) || bAnalyzedImage.IsLine(k, j))
                            {
                                ExpectedEdgeLocation =  k;
                                break;
                            }
                        }
                    }
                    if (bAnalyzedImage.InBitmapRangeX(ExpectedEdgeLocation + PixelOffsetArray[i]))
                    {
                        if (bAnalyzedImage.IsLine(ExpectedEdgeLocation + PixelOffsetArray[i], j))
                        {
                            if (!MiddleOfLine)
                            {
                                MiddleOfLine = true;
                                ConsecutiveLineValues++;
                            }
                        }
                    }
                }
                if (!MiddleOfLine || j == bAnalyzedImage.BottomLeft.y - 1)
                {
                    //we've reached the end of the line (or end of our scan).  add to the list
                    if (ConsecutiveLineValues > 3) //ignore spurious small lines
                    {
                        Line l = new Line(ConsecutiveLineValues, j - (ConsecutiveLineValues / 2), bAnalyzedImage.bitmap.Height - 1);
                        bAnalyzedImage.LeftHandSideLineList.Add(l);
                    }
                    ConsecutiveLineValues = 0;
                }
             }
        }
        public static List<AnalyzedImage> Analyze(List<Bitmap> Images, List<Color> BackgroundColors, List<Color> PaperColors)
        {
            /*
             * implementation ideas:
             * 
             * Track boundary-crossing lines (pen strokes that go across shredding lines)
             * Describe lines with polynomial equations, then use deviation from those equations when synthesizing (might work better with cursive than printing...)
             * 
            */
            int Index = 1;
            List<AnalyzedImage> result = new List<AnalyzedImage>();
            foreach (Bitmap b in Images)
            {
                AnalyzedImage bAnalyzedImage = new AnalyzedImage(b, String.Format("{0}", Index));
                Index++;
                bool[][] MatchesBackground = new bool[b.Width][];
                bool[][] MatchesPaper = new bool[b.Width][]; for (int i = 0; i < b.Width; i++)
                {
                    MatchesBackground[i] = new bool[b.Height];
                    MatchesPaper[i] = new bool[b.Height];
                }
                for (int i = 0; i < b.Width; i++)
                {
                    for (int j = 0; j < b.Height; j++)
                    {
                        Color PixelColor = b.GetPixel(i, j);

                        foreach(Color c in BackgroundColors)
                        {
                            if (Utilities.SameColor(PixelColor, c))
                            {
                                MatchesBackground[i][j] = true;    
                            }
                        }
                        foreach (Color c in PaperColors)
                        {
                            if (Utilities.SameColor(PixelColor, c))
                            {
                                MatchesPaper[i][j] = true;
                            }
                        }
                        if (MatchesBackground[i][j])
                        {
                            bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i, j, Color.Black);
                        }
                        else if (MatchesPaper[i][j])
                        { 
                            //color dark red
                            bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i, j, bAnalyzedImage.BlackAndWhitePaperColor);

                            //Mark upper left, right, bottom, top
                            if (i + j < bAnalyzedImage.TopLeft.Value(0, 0))
                            {
                                bAnalyzedImage.TopLeft.x = i;
                                bAnalyzedImage.TopLeft.y = j;
                            }
                            if ((bAnalyzedImage.bitmap.Width - i) + j < bAnalyzedImage.TopRight.Value(bAnalyzedImage.bitmap.Width, 0))
                            {
                                bAnalyzedImage.TopRight.x = i;
                                bAnalyzedImage.TopRight.y = j;
                            }
                            if (i + (bAnalyzedImage.bitmap.Height - j) < bAnalyzedImage.BottomLeft.Value(0, bAnalyzedImage.bitmap.Height))
                            {
                                bAnalyzedImage.BottomLeft.x = i;
                                bAnalyzedImage.BottomLeft.y = j;
                            }
                            if ((bAnalyzedImage.bitmap.Width - i) + (bAnalyzedImage.bitmap.Height - j) < bAnalyzedImage.BottomRight.Value(bAnalyzedImage.bitmap.Width, bAnalyzedImage.bitmap.Height))
                            {
                                bAnalyzedImage.BottomRight.x = i;
                                bAnalyzedImage.BottomRight.y = j;
                            }
                        }
                    }
                }
                for (int i = 0; i < b.Width; i++)
                {
                    for (int j = 0; j < b.Height; j++)
                    {
                        if(!MatchesPaper[i][j] && !MatchesBackground[i][j])
                        {
                            bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i, j, bAnalyzedImage.BlackAndWhiteLineColor);
                            if(SmoothLines)
                            {
                                if (i >= bAnalyzedImage.BlackAndWhiteBitmap.Width - 2 || i <= 1 || j >= bAnalyzedImage.BlackAndWhiteBitmap.Height - 2 || j <= 1)
                                {
                                 //Do nothing if we're on the edge of the image
                                }
                                else
                                {
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i + 1, j + 1, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i + 1, j - 1, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i - 1, j + 1, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i - 1, j - 1, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i + 1, j, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i, j + 1, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i, j - 1, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i - 1, j, bAnalyzedImage.BlackAndWhiteLineColor); 
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i + 2, j, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i - 2, j, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i, j + 2, bAnalyzedImage.BlackAndWhiteLineColor);
                                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(i, j - 2, bAnalyzedImage.BlackAndWhiteLineColor);
                                }
                            }
                        }
                    }
                }

                //Now scan the sides of image looking for lines
                FindLines(bAnalyzedImage);



                //do debug printing.  This isn't necessary, but it's helpful for us to verify
                bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(bAnalyzedImage.TopLeft.x, bAnalyzedImage.TopLeft.y, Color.GreenYellow);
                bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(bAnalyzedImage.TopRight.x, bAnalyzedImage.TopRight.y, Color.GreenYellow);
                bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(bAnalyzedImage.BottomRight.x, bAnalyzedImage.BottomRight.y, Color.GreenYellow);
                bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(bAnalyzedImage.BottomLeft.x, bAnalyzedImage.BottomLeft.y, Color.GreenYellow);


                int ExpectedEdgeLocation = (bAnalyzedImage.TopRight.x + bAnalyzedImage.BottomRight.x) / 2;
                foreach (Line l in bAnalyzedImage.RightHandSideLineList)
                {
                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(ExpectedEdgeLocation, l.y, Color.Blue);
                }
                ExpectedEdgeLocation = (bAnalyzedImage.TopLeft.x + bAnalyzedImage.BottomLeft.x) / 2;
                foreach (Line l in bAnalyzedImage.LeftHandSideLineList)
                {
                    bAnalyzedImage.BlackAndWhiteBitmap.SetPixel(ExpectedEdgeLocation, l.y, Color.Blue);
                }
                result.Add(bAnalyzedImage);
            }
            Loader.OutputAnalyzedBitmaps(result);
            return result;
        }
    }
}
