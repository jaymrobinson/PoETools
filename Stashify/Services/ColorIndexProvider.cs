using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Stashify.Services
{
    public class ColorIndexProvider : IColorIndexProvider
    {
        public IEnumerable<ColorIndexElement> GetIndexElements()
        {
            var elements = new List<ColorIndexElement>();
            string[] lines = File.ReadAllLines(Configuration.COLOR_INDEX_FILE);

            foreach (var line in lines)
            {
                string[] toks = line.Split(new char[] { '\t' });
                if (toks.Length == 4)
                {
                    ColorIndexElement elem = new ColorIndexElement(Color.FromArgb(Convert.ToByte(toks[1]), Convert.ToByte(toks[2]), Convert.ToByte(toks[3])), toks[0]);
                    elements.Add(elem);
                }
            }

            return elements;
        }
    }
}
