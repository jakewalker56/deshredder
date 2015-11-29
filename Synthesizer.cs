using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PetraAndJakesDeShredder
{
    class Synthesizer
    {
        public enum MatchType { LR, RL, LRUpsideDown, RLUpsideDown};
        public static float MinScoreThreshold = 0.8f;
        public static float ScoreLineSets(List<Line> lhs, List<Line> rhs)
        {
           /*
            Console.WriteLine("\tlhs:");
            Loader.PrintLineSet(lhs);
            Console.WriteLine("\trhs:"); 
            Loader.PrintLineSet(rhs);
            */

            if (lhs.Count == 0 || rhs.Count == 0)
                return 0;
            float TotalScore = 0.0f;
            int LineDifferenceTally = 0;
            foreach (Line lhsline in lhs)
            {
                float LineScore = 0.0f;
                //find closest line
                Line closestLine = new Line(0, 1000, 1000);
                foreach (Line rhsline in rhs)
                {
                    if (Math.Abs(lhsline.y - rhsline.y) < Math.Abs(lhsline.y - closestLine.y))
                    {
                        closestLine = rhsline;
                    }
                }
                
                //check to see if widths are close
                if (Math.Abs(closestLine.Width - lhsline.Width) < (float)10)
                {
                    LineScore += 0.1f;
                    LineScore += 0.1f * (10 - Math.Abs(closestLine.Width - lhsline.Width)) / (float)10;
                }

                //check total difference in line locations
                if (Math.Abs(closestLine.y - lhsline.y) < 20)
                {
                    LineScore += 0.2f;
                    LineScore += 0.2f * (20 - Math.Abs(closestLine.y - lhsline.y)) / (float)20;
                }

                if (Math.Abs(lhs.Count - rhs.Count) < 2)
                {
                    LineScore += 0.1f;
                    LineScore += 0.1f * (3 - Math.Abs(lhs.Count - rhs.Count)) / 3;
                }

                LineScore += (lhs.Count > 7 ? 0.2f : 0.2f * lhs.Count / 8) * Math.Min(lhs.Count, rhs.Count) / Math.Max(lhs.Count, rhs.Count);
                

                LineDifferenceTally += closestLine.y - lhsline.y;
                //keep tally of both difference and difference squared

                TotalScore += LineScore;
            }
            //TODO: use line slope as part of score- would need to do this where we have access to the TopRight/TopLeft etc. points

            if (LineDifferenceTally > rhs.Count * 5)
            { 
                //this might indicate systematic failure- need to shift rhs up by LineDifferenceTally
            }
            if (LineDifferenceTally < rhs.Count * -5)
            {
                //this might indicate systematic failure- need to shift rhs down by LineDifferenceTally
            }

            return TotalScore / (float)lhs.Count; //normalize.
        }
        public static List<Line> GetReversedLineList(List<Line> LineList)
        {
            List<Line> result = new List<Line>();
            foreach (Line line in LineList)
            {
                Line l = new Line(line.Width, line.MaxY - line.y, line.MaxY);
                result.Add(l);
            }
            return result;
        }
        public static float CompareAnalyzedImages(AnalyzedImage lhs, AnalyzedImage rhs, out MatchType matchType)
        {
            /*
             * Algorithm:
             *      check that both hieghts are close
             *      foreach of the 4 positions:
             *          compare edges of paper- check for distance between lines crossing edges, line widths, line counts, etc.
             *      assign score between 0 and 1
             *          
             * assumptions:
             *      Pieces are rectangular, and long sides match up with other long sides
             */


            matchType = MatchType.LR;

            // [L] [R]
            // [H] [H]
            // [S] [S]
            float LR = (ScoreLineSets(lhs.RightHandSideLineList, rhs.LeftHandSideLineList) + ScoreLineSets(rhs.LeftHandSideLineList, lhs.RightHandSideLineList)) / 2.0f;

            // [R] [L]
            // [H] [H]
            // [S] [S]
            float RL = (ScoreLineSets(rhs.RightHandSideLineList, lhs.LeftHandSideLineList) + ScoreLineSets(lhs.LeftHandSideLineList, rhs.RightHandSideLineList)) / 2.0f;

            // [L] [S]
            // [H] [H]
            // [S] [R]
            float LRUpsideDown = (ScoreLineSets(lhs.RightHandSideLineList, GetReversedLineList(rhs.RightHandSideLineList)) + ScoreLineSets(GetReversedLineList(rhs.RightHandSideLineList), lhs.RightHandSideLineList)) / 2.0f;

            // [S] [L]
            // [H] [H]
            // [R] [S]
            float RLUpsideDown = (ScoreLineSets(lhs.LeftHandSideLineList, GetReversedLineList(rhs.LeftHandSideLineList)) + ScoreLineSets(GetReversedLineList(rhs.LeftHandSideLineList), lhs.LeftHandSideLineList)) / 2.0f;

            float MaxScore = Math.Max(Math.Max(RL, LR), Math.Max(LRUpsideDown, RLUpsideDown));
            Console.WriteLine(String.Format("{0} <--> {1} Max Score: {2}", lhs.Name, rhs.Name, MaxScore));
            if (MaxScore == RL)
            {
                Console.WriteLine(string.Format("Matched RL with confidence: {0}", MaxScore));
                matchType = MatchType.RL;
            }
            else if (MaxScore == LR)
            {
                Console.WriteLine(string.Format("Matched LR with confidence: {0}", MaxScore));
                matchType = MatchType.LR;
            }
            else if (MaxScore == RLUpsideDown)
            {
                Console.WriteLine(string.Format("Matched RLUpsideDown with confidence: {0}", MaxScore));
                matchType = MatchType.RLUpsideDown;
            }
            else if (MaxScore == LRUpsideDown)
            {
                Console.WriteLine(string.Format("Matched LRUpsideDown with confidence: {0}", MaxScore));
                matchType = MatchType.LRUpsideDown;
            }

            return MaxScore;
        }
        public static AnalyzedImage CombineImages(AnalyzedImage lhs, AnalyzedImage rhs, MatchType matchType)
        {
            if (matchType == MatchType.LRUpsideDown || matchType == MatchType.RLUpsideDown)
            {
                //flip the rhs image upside down
                Bitmap flippedRhsBitmap = new Bitmap(rhs.bitmap.Width, rhs.bitmap.Height);
                Bitmap flippedBlackAndWhiteRhsBitmap = new Bitmap(rhs.BlackAndWhiteBitmap.Width, rhs.BlackAndWhiteBitmap.Height);

                for (int i = 0; i < flippedRhsBitmap.Width; i++)
                {
                    for (int j = 0; j < flippedRhsBitmap.Height; j++)
                    {
                        flippedRhsBitmap.SetPixel(flippedRhsBitmap.Width - i - 1, flippedRhsBitmap.Height - j - 1, rhs.bitmap.GetPixel(i, j));
                        flippedBlackAndWhiteRhsBitmap.SetPixel( flippedBlackAndWhiteRhsBitmap.Width - i - 1, flippedBlackAndWhiteRhsBitmap.Height - j - 1, rhs.BlackAndWhiteBitmap.GetPixel(i, j));
                    }
                }

                rhs.bitmap = flippedRhsBitmap;
                rhs.BlackAndWhiteBitmap = flippedBlackAndWhiteRhsBitmap;

                //flip and switch the Line Lists
                List<Line> newLeftLineList = GetReversedLineList(rhs.RightHandSideLineList);
                List<Line> newRightLineList = GetReversedLineList(rhs.LeftHandSideLineList);
                rhs.LeftHandSideLineList = newLeftLineList;
                rhs.RightHandSideLineList = newRightLineList;
            }
            if (matchType == MatchType.LR || matchType == MatchType.LRUpsideDown)
            {
                lhs.Name = lhs.Name + "-" + rhs.Name;
                lhs.RightHandSideLineList = rhs.RightHandSideLineList;
                lhs.TopRight = new Point(rhs.TopRight.x + lhs.bitmap.Width, rhs.TopRight.y);
                lhs.BottomRight = new Point(rhs.BottomRight.x + lhs.bitmap.Width, rhs.BottomRight.y);

               Bitmap newBitmap = new Bitmap(lhs.bitmap.Width + rhs.bitmap.Width, Math.Max(lhs.bitmap.Height, rhs.bitmap.Height));
                Bitmap newBlackAndWhiteBitmap = new Bitmap(lhs.bitmap.Width + rhs.bitmap.Width, Math.Max(lhs.bitmap.Height, rhs.bitmap.Height));
                for (int i = 0; i < newBitmap.Width; i++)
                {
                    for (int j = 0; j < newBitmap.Height; j++)
                    {
                        Color pixelColor = Color.Black;
                        Color BlackAndWhitePixelColor = Color.Black;
                        if (i < lhs.bitmap.Width)
                        {
                            if (lhs.InBitmapRangeY(j))
                            {
                                pixelColor = lhs.bitmap.GetPixel(i, j);
                                BlackAndWhitePixelColor = lhs.BlackAndWhiteBitmap.GetPixel(i, j);
                            }
                        }
                        else
                        {
                            if (rhs.InBitmapRangeY(j))
                            {
                                pixelColor = rhs.bitmap.GetPixel(i - lhs.bitmap.Width, j);
                                BlackAndWhitePixelColor = rhs.BlackAndWhiteBitmap.GetPixel(i - lhs.BlackAndWhiteBitmap.Width, j);
                            }
                        }
                        newBitmap.SetPixel(i, j, pixelColor);
                        newBlackAndWhiteBitmap.SetPixel(i, j, BlackAndWhitePixelColor);
                    }
                 }
                lhs.bitmap = newBitmap;
                lhs.BlackAndWhiteBitmap = newBlackAndWhiteBitmap;
                
            }
            else if (matchType == MatchType.RL || matchType == MatchType.RLUpsideDown)
            {
                lhs.Name = rhs.Name + "-" + lhs.Name;
                lhs.LeftHandSideLineList = rhs.LeftHandSideLineList;
                lhs.TopLeft = rhs.TopLeft;
                lhs.TopRight = new Point(lhs.TopRight.x + rhs.bitmap.Width, lhs.TopRight.y);
                lhs.BottomLeft = rhs.BottomLeft;
                lhs.BottomRight = new Point(lhs.BottomRight.x + rhs.bitmap.Width, lhs.BottomRight.y);

                Bitmap newBitmap = new Bitmap(lhs.bitmap.Width + rhs.bitmap.Width, Math.Max(lhs.bitmap.Height, rhs.bitmap.Height));
                Bitmap newBlackAndWhiteBitmap = new Bitmap(lhs.bitmap.Width + rhs.bitmap.Width, Math.Max(lhs.bitmap.Height, rhs.bitmap.Height));
                for (int i = 0; i < newBitmap.Width; i++)
                {
                    for (int j = 0; j < newBitmap.Height; j++)
                    {
                        Color pixelColor = Color.Black;
                        Color BlackAndWhitePixelColor = Color.Black;
                        if (i < rhs.bitmap.Width)
                        {
                            if (rhs.InBitmapRangeY(j))
                            {
                                pixelColor = rhs.bitmap.GetPixel(i, j);
                                BlackAndWhitePixelColor = rhs.BlackAndWhiteBitmap.GetPixel(i, j);
                            }
                        }
                        else
                        {
                            if (lhs.InBitmapRangeY(j))
                            {
                                pixelColor = lhs.bitmap.GetPixel(i - rhs.bitmap.Width, j);
                                BlackAndWhitePixelColor = lhs.BlackAndWhiteBitmap.GetPixel(i - rhs.bitmap.Width, j);
                            }
                        }
                        newBitmap.SetPixel(i, j, pixelColor);
                        newBlackAndWhiteBitmap.SetPixel(i, j, BlackAndWhitePixelColor);
                    }
                }

                lhs.bitmap = newBitmap;
                lhs.BlackAndWhiteBitmap = newBlackAndWhiteBitmap;
            }
           
            return lhs; //should combine the bitmaps, the Black and White bitmaps, and select the correct upper right/lowerleft ect. points
            
        }

        public static void Synthesize(AnalyzedImage[] AnalyzedImageArray)
        {
            for (int i = 0; i < AnalyzedImageArray.Length; i++)
            {
                if (AnalyzedImageArray[i] == null)
                {
                    continue;
                }
                float MaxScore = 0.0f;
                int MaxScoreImageIndex = 0;
                MatchType MaxScoreMatchType = MatchType.LR; 
                for (int j = 0; j < AnalyzedImageArray.Length; j++)
                  {
                        if(i != j && AnalyzedImageArray[j] != null)
                        {
                            MatchType matchType;
                            float score = CompareAnalyzedImages(AnalyzedImageArray[i], AnalyzedImageArray[j], out matchType);
                            if (score > MinScoreThreshold && score > MaxScore)
                            {
                                MaxScore = score;
                                MaxScoreImageIndex = j;
                                MaxScoreMatchType = matchType;
                            }
                        }
                  }
                  if (MaxScore > MinScoreThreshold)
                  {
                      AnalyzedImageArray[i] = CombineImages(AnalyzedImageArray[i], AnalyzedImageArray[MaxScoreImageIndex], MaxScoreMatchType);
                      AnalyzedImageArray[MaxScoreImageIndex] = null;
                      i--;
                  }
            }
            List<AnalyzedImage> result = new List<AnalyzedImage>();
            for (int i = 0; i < AnalyzedImageArray.Length; i++)
            { 
                if(AnalyzedImageArray[i] != null)
                    result.Add(AnalyzedImageArray[i]);
            }
            Loader.OutputSynthesizedBitmaps(result);
        }
    }
}
