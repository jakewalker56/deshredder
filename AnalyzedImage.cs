using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PetraAndJakesDeShredder
{
    class AnalyzedImage
    {
        public Color BlackAndWhiteLineColor = Color.White;
        public Color BlackAndWhitePaperColor = Color.DarkRed;
        public Bitmap bitmap;
        public Bitmap BlackAndWhiteBitmap;
        public Point TopLeft;
        public Point TopRight;
        public Point BottomRight;
        public Point BottomLeft;
        public List<Line> RightHandSideLineList;
        public List<Line> LeftHandSideLineList;
        public string Name;

        public bool InBitmapRangeX(int x)
        {
            return x >= 0 && x < bitmap.Width;
        }
        public bool InBitmapRangeY(int y)
        {
            return y >= 0 && y < bitmap.Height;
        }
        public bool IsLine(int x, int y)
        {
            Color color = BlackAndWhiteBitmap.GetPixel(x, y);
            bool ColorMatch = (color.R == BlackAndWhiteLineColor.R && color.G == BlackAndWhiteLineColor.G && color.B == BlackAndWhiteLineColor.B);
            return ColorMatch;
        }
        public bool IsPaper(int x, int y)
        {
            Color color = BlackAndWhiteBitmap.GetPixel(x, y);
            bool ColorMatch = (color.R == BlackAndWhitePaperColor.R && color.G == BlackAndWhitePaperColor.G && color.B == BlackAndWhitePaperColor.B);
            return ColorMatch;
        }
        public AnalyzedImage(Bitmap Image, string name)
        {
            bitmap = Image;
            BlackAndWhiteBitmap = new Bitmap(bitmap);
            RightHandSideLineList = new List<Line>();
            LeftHandSideLineList = new List<Line>();
            BottomRight = new Point(0, 0);
            BottomLeft = new Point(Image.Width - 1, 0);
            TopRight = new Point(0, Image.Height - 1);
            TopLeft = new Point(Image.Width - 1, Image.Height - 1);
            Name = name;
        }
    }
}
