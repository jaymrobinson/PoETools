using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Stashify.Services
{
    public class ColorIndexBuilder : IColorIndexBuilder
    {
        public void Build()
        {
            Console.WriteLine("Building color index...");

            var output = new List<ColorIndexElement>();

            float maxBrightness = ProcessIndices(output);

            SaveIndices(output, maxBrightness);
        }

        private static float ProcessIndices(List<ColorIndexElement> output)
        {
            var files = Directory.EnumerateFiles(Configuration.ASSET_DIR);
            float maxBrightness = 0;
            foreach (var file in files)
            {
                var img = Image.FromFile(file);
                var bmp = new Bitmap(img);

                double rTotal = 0, gTotal = 0, bTotal = 0;

                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        var pxColor = bmp.GetPixel(i, j);
                        var aFactor = (double)pxColor.A / 255;
                        rTotal += pxColor.R * aFactor;
                        gTotal += pxColor.G * aFactor;
                        bTotal += pxColor.B * aFactor;
                    }
                }

                Console.WriteLine(file);

                var pxCount = bmp.Width * bmp.Height;
                var avgColor = Color.FromArgb((int)(rTotal / pxCount), (int)(gTotal / pxCount), (int)(bTotal / pxCount));

                var hslColor = HSLColor.FromRGB(avgColor);

                if (hslColor.Luminosity > maxBrightness)
                {
                    maxBrightness = hslColor.Luminosity;
                }

                output.Add(new ColorIndexElement(avgColor, new FileInfo(file).Name));
            }

            return maxBrightness;
        }

        private static void SaveIndices(List<ColorIndexElement> output, float maxBrightness)
        {
            using (var tw = File.CreateText(Configuration.COLOR_INDEX_FILE))
            {
                var lumBoost = 1.0f / maxBrightness;
                foreach (var elem in output)
                {
                    var hsl = HSLColor.FromRGB(elem.Color);
                    var newHsl = new HSLColor(hsl.Hue, hsl.Saturation, hsl.Luminosity * lumBoost);
                    var newRgb = newHsl.ToRGB();
                    tw.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}",
                        elem.Filename,
                        newRgb.R,
                        newRgb.G,
                        newRgb.B
                    ));
                }
            }
        }
    }
}
