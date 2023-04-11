using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfFireworks.Utilities
{
    internal static class ColorBand
    {
        private static IEnumerable<Color> GetAllHsvColors()
        {
            // R:255, G:0-255, B:0
            for (int g = 0; g < 256; g++)
                yield return Color.FromRgb(255, (byte)g, 0);

            // R:255-0, G:255, B:0
            for (int r = 255; r >= 0; r--)
                yield return Color.FromRgb((byte)r, 255, 0);

            // R:0, G:255, B:0-255
            for (int b = 0; b < 256; b++)
                yield return Color.FromRgb(0, 255, (byte)b);

            // R:0, G:255-0, B:255
            for (int g = 255; g >= 0; g--)
                yield return Color.FromRgb(0, (byte)g, 255);

            // R:0-255, G:0, B:255
            for (int r = 0; r < 256; r++)
                yield return Color.FromRgb((byte)r, 0, 255);

            // R:255, G:0, B:255-0
            for (int b = 255; b >= 0; b--)
                yield return Color.FromRgb(255, 0, (byte)b);
        }

        private static Color[] _allHsvColors =
            GetAllHsvColors().ToArray();

        public static Color GetColor(double rate)
        {
            rate %= 1;

            return _allHsvColors[(int)(_allHsvColors.Length * rate)];
        }
    }
}
