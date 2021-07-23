using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InlineColorPicker
{
    public class ColorInfo
    {
        public Color? Color;
        public bool WasSpecifiedWithAlpha;

        public static ColorInfo White = new ColorInfo() { Color = Colors.White, WasSpecifiedWithAlpha = true };
    }
}
