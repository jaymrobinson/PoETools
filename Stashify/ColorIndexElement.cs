using System.Drawing;

namespace Stashify
{
    public class ColorIndexElement
    {
        internal Color Color;
        internal string Filename;
        internal float Hue;       

        internal ColorIndexElement(Color color, string filename)
        {
            Color = color;
            Hue = color.GetHue();            
            Filename = filename;
        }

    }
}
