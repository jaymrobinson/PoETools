using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Stashify.Services
{
    public class ImageGenerationService : IImageGenerationService
    {
        private ImageResizer imageResizer;

        public ImageGenerationService()
        {
            imageResizer = new ImageResizer();
        }

        public void Generate(string fileName)
        {
            var img = Image.FromFile(fileName);

            var width = Configuration.IMAGE_WIDTH;
            var height = Configuration.IMAGE_HEIGHT;

            if (Configuration.PRESERVE_ASPECT_RATIO)
            {
                double ratioX = (double)Configuration.IMAGE_WIDTH / img.Width;
                double ratioY = (double)Configuration.IMAGE_HEIGHT / img.Height;
                double ratio = ratioX < ratioY ? ratioX : ratioY;
                width = Convert.ToInt32(img.Width * ratio);
                height = Convert.ToInt32(img.Height * ratio);
            }

            var bmp = imageResizer.Resize(img, width, height);

            var indexElements = GetIndexElements();

            var outputBitmap = new Bitmap(bmp.Width * 32, bmp.Height * 32);
            var blackImage = Image.FromFile("black.png");
            using (var g = Graphics.FromImage(outputBitmap))
            {
                PopulateImages(bmp, indexElements, blackImage, g);
                
                if (Configuration.DRAW_GRID_LINES)
                {
                    PopulateGridLines(bmp, g);
                }
                outputBitmap.Save(string.Format("Stashified_{0:yyyyMMdd-HHmmss}.png", DateTime.Now));
            }
        }

        private void PopulateImages(Bitmap bmp, List<ColorIndexElement> indexElements, Image blackImage, Graphics g)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {

                    var color = bmp.GetPixel(i, j);
                    var colorFile = FindClosestColorFile(color, indexElements, Configuration.PRIORITIZE_HUE);
                    var colorImgPath = Path.Combine(Configuration.ASSET_DIR, colorFile);
                    var colorImg = Image.FromFile(colorImgPath);
                    var colorBmp = imageResizer.Resize(colorImg, 32, 32);

                    g.DrawImage(blackImage, i * 32, j * 32);
                    g.DrawImage(colorBmp, i * 32, j * 32);

                }
            }
        }

        private static void PopulateGridLines(Bitmap bmp, Graphics g)
        {
            using (var p = new Pen(Color.Gray))
            {
                for (int k = 0; k < bmp.Width; k++)
                {
                    g.DrawLine(p, k * 32, 0, k * 32, bmp.Height * 32 - 1);
                }
                for (int l = 0; l < bmp.Height; l++)
                {
                    g.DrawLine(p, 0, l * 32, bmp.Width * 32 - 1, l * 32);
                }
            }
        }

        private string FindClosestColorFile(Color color, List<ColorIndexElement> index, bool prioritizeHue)
        {
            double minDist = 1000;
            string file = "";
            foreach (var elem in index)
            {
                double dist =
                    (elem.Color.R - color.R) * (elem.Color.R - color.R) +
                    (elem.Color.G - color.G) * (elem.Color.G - color.G) +
                    (elem.Color.B - color.B) * (elem.Color.B - color.B);
                if (prioritizeHue)
                {
                    dist += Math.Pow(DegreeDifference(elem.Hue, color.GetHue()), 2);
                }
                dist = Math.Sqrt(dist);
                if (dist < minDist)
                {
                    minDist = dist;
                    file = elem.Filename;
                }
            }
            return file;
        }

        private float DegreeDifference(float a, float b)
        {
            float diff = a - b;
            diff = (diff + 180) % 360 - 180;
            return diff;
        }
    }
}
