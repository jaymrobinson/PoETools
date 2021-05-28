using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Configuration;
using System.Net;
using System.Threading;

namespace Stashify
{
    class Program
    { 
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Usage: ");
                Console.WriteLine("");
                Console.WriteLine("Stashify.exe /D");
                Console.WriteLine("Stashify.exe /I");
                Console.WriteLine("Stashify.exe inputFile");
                Console.WriteLine("");
                Console.WriteLine("  /D          Downloads assets specified in assets.txt");
                Console.WriteLine("  /I          Builds the color index (ColorIndex.tsv) from");
                Console.WriteLine("               the assets contained in the specified AssetFilePath");
                Console.WriteLine("               in config");
                Console.WriteLine("  inputFile   Stashifies the specified image file");
                Console.WriteLine("");
                Console.WriteLine("  Check the config file for further options.");
                Console.WriteLine("");
                Console.WriteLine("Press any key to continue...");
                Console.ReadLine();
                Environment.Exit(0);
            }

            string assetDir = ConfigurationManager.AppSettings["AssetFilePath"] ?? "assets";

            if (String.Compare(args[0],"/d", true) == 0)
            {
                DownloadAssets(assetDir);
                Environment.Exit(0);
            }
            if(String.Compare(args[0], "/i", true) == 0)
            {
                BuildColorIndex(assetDir);
                Environment.Exit(0);
            }

            bool drawGridLines = Convert.ToBoolean(ConfigurationManager.AppSettings["DrawGridLines"] ?? "false");
            int horizRes = Convert.ToInt32(ConfigurationManager.AppSettings["HorizontalResolution"] ?? "74");
            int vertRes = Convert.ToInt32(ConfigurationManager.AppSettings["VerticalResolution"] ?? "74");
            bool prioritizeHue = Convert.ToBoolean(ConfigurationManager.AppSettings["PrioritizeHue"] ?? "true");
            bool preserveAspectRatio = Convert.ToBoolean(ConfigurationManager.AppSettings["PreserveAspectRatio"] ?? "true");
            string filename = args[0];

            Image img = Image.FromFile(filename);
            if (preserveAspectRatio)
            {
                double ratioX = (double)horizRes / img.Width;
                double ratioY = (double)vertRes / img.Height;
                double ratio = ratioX < ratioY ? ratioX : ratioY;
                horizRes = Convert.ToInt32(img.Width * ratio);
                vertRes = Convert.ToInt32(img.Height * ratio);
            }
            Bitmap bmp = ResizeImage(img, horizRes, vertRes);

            if(!Directory.Exists(assetDir))
            {
                Console.WriteLine("Specified AssetFilePath folder not found.");
                Environment.Exit(-1);
            }

            if(!File.Exists("ColorIndex.tsv"))
            {
                Console.WriteLine("ColorIndex.tsv not found.");
                Environment.Exit(-2);
            }

            List<ColorIndexElement> index = new List<ColorIndexElement>();
            string[] lines = File.ReadAllLines("ColorIndex.tsv");
            
            foreach(string line in lines)
            {
                string[] toks = line.Split(new char[] { '\t' });
                if(toks.Length == 4)
                {
                    ColorIndexElement elem = new ColorIndexElement(Color.FromArgb(Convert.ToByte(toks[1]), Convert.ToByte(toks[2]), Convert.ToByte(toks[3])), toks[0]);
                    index.Add(elem);
                }
            }

            Bitmap outputBitmap = new Bitmap(bmp.Width * 32, bmp.Height * 32);
            Image blackImage = Image.FromFile("black.png");
            using (Graphics g = Graphics.FromImage(outputBitmap))
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        
                        Color color = bmp.GetPixel(i, j);
                        string colorFile = FindClosestColorFile(color, index, prioritizeHue);
                        string colorImgPath = Path.Combine(assetDir, colorFile);
                        Image colorImg = Image.FromFile(colorImgPath);
                        Bitmap colorBmp = ResizeImage(colorImg, 32, 32);

                        g.DrawImage(blackImage, i * 32, j * 32);
                        g.DrawImage(colorBmp, i * 32, j * 32);

                    }
                }
                if (drawGridLines)
                {
                    using (Pen p = new Pen(Color.Gray))
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
            }
            outputBitmap.Save(String.Format("Stashified_{0:yyyyMMdd-HHmmss}.png", DateTime.Now));
        }

        static void DownloadAssets(string assetDir)        
        {
            if(!File.Exists("assets.txt"))
            {
                Console.WriteLine("Asset file (assets.txt) not found.");
                Environment.Exit(-3);
            }

            if(!Directory.Exists(assetDir))
            {
                try
                {
                    Directory.CreateDirectory(assetDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Couldn't create asset directory. " + ex.Message);
                    Environment.Exit(-4);
                }
            }
            string[] lines = File.ReadAllLines("assets.txt");            
            WebClient client = new WebClient();            
            foreach (var line in lines)
            {                
                Console.WriteLine("Downloading: " + line);
                string filename = line.Split(new char[] { '/' }).LastOrDefault();
                client.DownloadFile(line, Path.Combine("assets", filename));
                Thread.Sleep(500);
            }
            client.Dispose();
        }

        static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        static string FindClosestColorFile(Color color, List<ColorIndexElement> index, bool prioritizeHue)
        {
            double minDist = 1000;
            string file = "";
            foreach(var elem in index)
            {
                double dist =
                    (elem.Color.R - color.R) * (elem.Color.R - color.R) +
                    (elem.Color.G - color.G) * (elem.Color.G - color.G) +
                    (elem.Color.B - color.B) * (elem.Color.B - color.B);
                if(prioritizeHue)
                {
                    dist += Math.Pow(DegreeDifference(elem.Hue, color.GetHue()), 2);
                }
                dist = Math.Sqrt(dist);
                if(dist < minDist)
                {
                    minDist = dist;
                    file = elem.Filename;
                }
            }
            return file;
        }

        static float DegreeDifference(float a, float b)
        {
            float diff = a - b;
            diff = (diff + 180) % 360 - 180;
            return diff;
        }

        static void BuildColorIndex(string assetDir)
        {
            if (!Directory.Exists(assetDir))
            {
                Console.WriteLine("Specified AssetFilePath folder not found.");
                Environment.Exit(-1);
            }

            Console.WriteLine("Building color index...");
            var files = Directory.EnumerateFiles(assetDir);

            List<ColorIndexElement> output = new List<ColorIndexElement>();
            float maxBrightness = 0;
            float rMax = 0;
            float gMax = 0;
            float bMax = 0;
            foreach (var file in files)
            {
                Image img = Image.FromFile(file);
                Bitmap bmp = new Bitmap(img);
                double rTotal = 0, gTotal = 0, bTotal = 0;

                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        Color pxColor = bmp.GetPixel(i, j);
                        double aFactor = (double)pxColor.A / 255;
                        rTotal += pxColor.R * aFactor;
                        gTotal += pxColor.G * aFactor;
                        bTotal += pxColor.B * aFactor;
                    }
                }
                Console.WriteLine(file);
                int pxCount = bmp.Width * bmp.Height;
                FileInfo info = new FileInfo(file);
                Color avgColor = Color.FromArgb((int)(rTotal / pxCount), (int)(gTotal / pxCount), (int)(bTotal / pxCount));
                if (avgColor.R > rMax)
                {
                    rMax = avgColor.R;
                }
                if (avgColor.G > gMax)
                {
                    gMax = avgColor.G;
                }
                if (avgColor.B > bMax)
                {
                    bMax = avgColor.B;
                }
                var hslColor = HSLColor.FromRGB(avgColor);

                if (hslColor.Luminosity > maxBrightness)
                {
                    maxBrightness = hslColor.Luminosity;
                }
                output.Add(new ColorIndexElement(avgColor, info.Name));
            }
            TextWriter tw = File.CreateText("ColorIndex.tsv");
            float lumBoost = 1 / maxBrightness;
            foreach (var elem in output)
            {
                var hsl = HSLColor.FromRGB(elem.Color);
                var newHsl = new HSLColor(hsl.Hue, hsl.Saturation, hsl.Luminosity * lumBoost);
                var newRgb = newHsl.ToRGB();
                tw.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}", 
                    elem.Filename,
                    newRgb.R,
                    newRgb.G,
                    newRgb.B
                    ));
            }
            tw.Close();
        }
    }

    class ColorIndexElement
    {
        internal Color Color;
        internal string Filename;
        internal float Hue;       

        internal ColorIndexElement(Color color, string filename)
        {
            this.Color = color;
            this.Hue = color.GetHue();            
            this.Filename = filename;
        }

    }
}
