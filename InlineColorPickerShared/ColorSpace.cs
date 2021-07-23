// Based on code from Page Brooks, Website: http://www.pagebrooks.com, RSS Feed: http://feeds.pagebrooks.com/pagebrooks
// Modified by nokola (http://nokola.com) to include rgv to hsv color space translation

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using InlineColorPicker;
using System.Text.RegularExpressions;
using System.Text;

namespace EasyPainter.Imaging.Silverlight
{
    internal class ColorSpace
    {

        internal static string[] NamedColors = new string[] {
            "AliceBlue", "#FFF0F8FF", "AntiqueWhite", "#FFFAEBD7", "Aqua", "#FF00FFFF", "Aquamarine", "#FF7FFFD4",
            "Azure", "#FFF0FFFF", "Beige", "#FFF5F5DC", "Bisque", "#FFFFE4C4", "Black", "#FF000000",
            "BlanchedAlmond", "#FFFFEBCD", "Blue", "#FF0000FF", "BlueViolet", "#FF8A2BE2", "Brown", "#FFA52A2A",
            "BurlyWood", "#FFDEB887", "CadetBlue", "#FF5F9EA0", "Chartreuse", "#FF7FFF00", "Chocolate", "#FFD2691E",
            "Coral", "#FFFF7F50", "CornflowerBlue", "#FF6495ED", "Cornsilk", "#FFFFF8DC", "Crimson", "#FFDC143C",
            "Cyan", "#FF00FFFF", "DarkBlue", "#FF00008B", "DarkCyan", "#FF008B8B", "DarkGoldenrod", "#FFB8860B",
            "DarkGray", "#FFA9A9A9", "DarkGreen", "#FF006400", "DarkKhaki", "#FFBDB76B", "DarkMagenta", "#FF8B008B",
            "DarkOliveGreen", "#FF556B2F", "DarkOrange", "#FFFF8C00", "DarkOrchid", "#FF9932CC", "DarkRed", "#FF8B0000",
            "DarkSalmon", "#FFE9967A", "DarkSeaGreen", "#FF8FBC8F", "DarkSlateBlue", "#FF483D8B", "DarkSlateGray", "#FF2F4F4F",
            "DarkTurquoise", "#FF00CED1", "DarkViolet", "#FF9400D3", "DeepPink", "#FFFF1493", "DeepSkyBlue", "#FF00BFFF",
            "DimGray", "#FF696969", "DodgerBlue", "#FF1E90FF", "Firebrick", "#FFB22222", "FloralWhite", "#FFFFFAF0",
            "ForestGreen", "#FF228B22", "Fuchsia", "#FFFF00FF", "Gainsboro", "#FFDCDCDC", "GhostWhite", "#FFF8F8FF",
            "Gold", "#FFFFD700", "Goldenrod", "#FFDAA520", "Gray", "#FF808080", "Green", "#FF008000",
            "GreenYellow", "#FFADFF2F", "Honeydew", "#FFF0FFF0", "HotPink", "#FFFF69B4", "IndianRed", "#FFCD5C5C",
            "Indigo", "#FF4B0082", "Ivory", "#FFFFFFF0", "Khaki", "#FFF0E68C", "Lavender", "#FFE6E6FA",
            "LavenderBlush", "#FFFFF0F5", "LawnGreen", "#FF7CFC00", "LemonChiffon", "#FFFFFACD", "LightBlue", "#FFADD8E6",
            "LightCoral", "#FFF08080", "LightCyan", "#FFE0FFFF", "LightGoldenrodYellow", "#FFFAFAD2", "LightGray", "#FFD3D3D3",
            "LightGreen", "#FF90EE90", "LightPink", "#FFFFB6C1", "LightSalmon", "#FFFFA07A", "LightSeaGreen", "#FF20B2AA",
            "LightSkyBlue", "#FF87CEFA", "LightSlateGray", "#FF778899", "LightSteelBlue", "#FFB0C4DE", "LightYellow", "#FFFFFFE0",
            "Lime", "#FF00FF00", "LimeGreen", "#FF32CD32", "Linen", "#FFFAF0E6", "Magenta", "#FFFF00FF",
            "Maroon", "#FF800000", "MediumAquamarine", "#FF66CDAA", "MediumBlue", "#FF0000CD", "MediumOrchid", "#FFBA55D3",
            "MediumPurple", "#FF9370DB", "MediumSeaGreen", "#FF3CB371", "MediumSlateBlue", "#FF7B68EE", "MediumSpringGreen", "#FF00FA9A",
            "MediumTurquoise", "#FF48D1CC", "MediumVioletRed", "#FFC71585", "MidnightBlue", "#FF191970", "MintCream", "#FFF5FFFA",
            "MistyRose", "#FFFFE4E1", "Moccasin", "#FFFFE4B5", "NavajoWhite", "#FFFFDEAD", "Navy", "#FF000080",
            "OldLace", "#FFFDF5E6", "Olive", "#FF808000", "OliveDrab", "#FF6B8E23", "Orange", "#FFFFA500",
            "OrangeRed", "#FFFF4500", "Orchid", "#FFDA70D6", "PaleGoldenrod", "#FFEEE8AA", "PaleGreen", "#FF98FB98",
            "PaleTurquoise", "#FFAFEEEE", "PaleVioletRed", "#FFDB7093", "PapayaWhip", "#FFFFEFD5", "PeachPuff", "#FFFFDAB9",
            "Peru", "#FFCD853F", "Pink", "#FFFFC0CB", "Plum", "#FFDDA0DD", "PowderBlue", "#FFB0E0E6",
            "Purple", "#FF800080", "Red", "#FFFF0000", "RosyBrown", "#FFBC8F8F", "RoyalBlue", "#FF4169E1",
            "SaddleBrown", "#FF8B4513", "Salmon", "#FFFA8072", "SandyBrown", "#FFF4A460", "SeaGreen", "#FF2E8B57",
            "SeaShell", "#FFFFF5EE", "Sienna", "#FFA0522D", "Silver", "#FFC0C0C0", "SkyBlue", "#FF87CEEB",
            "SlateBlue", "#FF6A5ACD", "SlateGray", "#FF708090", "Snow", "#FFFFFAFA", "SpringGreen", "#FF00FF7F",
            "SteelBlue", "#FF4682B4", "Tan", "#FFD2B48C", "Teal", "#FF008080", "Thistle", "#FFD8BFD8",
            "Tomato", "#FFFF6347", "Transparent", "#00FFFFFF", "Turquoise", "#FF40E0D0", "Violet", "#FFEE82EE",
            "Wheat", "#FFF5DEB3", "White", "#FFFFFFFF", "WhiteSmoke", "#FFF5F5F5", "Yellow", "#FFFFFF00",
            "YellowGreen", "#FF9ACD32" };

        private const byte MIN = 0;
        private const byte MAX = 255;

        internal static Dictionary<string, string> HexFromColorUpperCase = new Dictionary<string, string>();
        internal static Dictionary<string, string> ColorFromHex = new Dictionary<string, string>();
        private static Regex MatchNamedColorRegex;
        //private static Regex MatchNamedColorFreeFloatingRegex;

        static ColorSpace()
        {
            StringBuilder stringBuilder = new StringBuilder("[\"']\\b(");
            int num = NamedColors.Length / 2;
            for (int i = 0; i < num; i++)
            {
                HexFromColorUpperCase[NamedColors[i * 2].ToUpperInvariant()] = NamedColors[i * 2 + 1];
                ColorFromHex[NamedColors[i * 2 + 1]] = NamedColors[i * 2];
                stringBuilder.Append(NamedColors[i * 2]);
                if (i < num - 1)
                {
                    stringBuilder.Append("|");
                }
            }
            stringBuilder.Append(")\\b");
            MatchNamedColorRegex = new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            //MatchNamedColorFreeFloatingRegex = new Regex(stringBuilder.Remove(0, 4).ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        internal static MatchCollection MatchNamedColor(string text, bool shouldAllowFreeFloatingColorNames)
        {
            //if (shouldAllowFreeFloatingColorNames)
            //{
            //    return MatchNamedColorFreeFloatingRegex.Matches(text);
            //}

            return MatchNamedColorRegex.Matches(text);
        }

        public static Color GetColorFromPosition(int position)
        {
            byte mod = (byte)(position % MAX);
            byte diff = (byte)(MAX - mod);
            byte alpha = 255;

            switch (position / MAX)
            {
                case 0: return Color.FromArgb(alpha, MAX, mod, MIN);
                case 1: return Color.FromArgb(alpha, diff, MAX, MIN);
                case 2: return Color.FromArgb(alpha, MIN, MAX, mod);
                case 3: return Color.FromArgb(alpha, MIN, diff, MAX);
                case 4: return Color.FromArgb(alpha, mod, MIN, MAX);
                case 5: return Color.FromArgb(alpha, MAX, MIN, diff);
                default: return Colors.Black;
            }
        }

        internal static string GetHexCodeOrName(ColorInfo colorInfo)
        {
            Color c = colorInfo.Color.Value;

            string text;
            string alphaText = string.Format("#{0}{1}{2}{3}",
                    c.A.ToString("X2"),
                    c.R.ToString("X2"),
                    c.G.ToString("X2"),
                    c.B.ToString("X2"));

            if (colorInfo.WasSpecifiedWithAlpha || (c.A < 255))
            {
                text = alphaText;
            }
            else
            {
                text = string.Format("#{0}{1}{2}",
                    c.R.ToString("X2"),
                    c.G.ToString("X2"),
                    c.B.ToString("X2"));
            }

            string result;
            if (ColorFromHex.TryGetValue(alphaText, out result))
            {
                return result;
            }

            return text;
        }

        public static void ConvertRgbToHsv(double r, double g, double b, out double h, out double s, out double v)
        {
            double colorMax = Math.Max(Math.Max(r, g), b);

            v = colorMax;
            if (v == 0)
            {
                h = 0;
                s = 0;
                return;
            }

            // normalize
            r /= v;
            g /= v;
            b /= v;

            double colorMin = Math.Min(Math.Min(r, g), b);
            colorMax = Math.Max(Math.Max(r, g), b);

            s = colorMax - colorMin;
            if (s == 0)
            {
                h = 0;
                return;
            }

            // normalize saturation
            r = (r - colorMin) / s;
            g = (g - colorMin) / s;
            b = (b - colorMin) / s;
            colorMax = Math.Max(Math.Max(r, g), b);

            // calculate hue
            if (colorMax == r)
            {
                h = 0.0 + 60.0 * (g - b);
                if (h < 0.0)
                {
                    h += 360.0;
                }
            }
            else if (colorMax == g)
            {
                h = 120.0 + 60.0 * (b - r);
            }
            else // colorMax == b
            {
                h = 240.0 + 60.0 * (r - g);
            }

        }

        // Algorithm ported from: http://www.colorjack.com/software/dhtml+color+picker.html
        public static Color ConvertHsvToRgb(float h, float s, float v)
        {
            h = h / 360;
            if (s > 0)
            {
                if (h >= 1)
                    h = 0;
                h = 6 * h;
                int hueFloor = (int)Math.Floor(h);
                byte a = (byte)Math.Round(MAX * v * (1.0 - s));
                byte b = (byte)Math.Round(MAX * v * (1.0 - (s * (h - hueFloor))));
                byte c = (byte)Math.Round(MAX * v * (1.0 - (s * (1.0 - (h - hueFloor)))));
                byte d = (byte)Math.Round(MAX * v);

                switch (hueFloor)
                {
                    case 0: return Color.FromArgb(MAX, d, c, a);
                    case 1: return Color.FromArgb(MAX, b, d, a);
                    case 2: return Color.FromArgb(MAX, a, d, c);
                    case 3: return Color.FromArgb(MAX, a, b, d);
                    case 4: return Color.FromArgb(MAX, c, a, d);
                    case 5: return Color.FromArgb(MAX, d, a, b);
                    default: return Color.FromArgb(0, 0, 0, 0);
                }
            }
            else
            {
                byte d = (byte)(v * MAX);
                return Color.FromArgb(255, d, d, d);
            }
        }
    }
}

