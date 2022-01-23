using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapBase {
    public static class BinColor {
        const int _gradientCnt = 25;
        static Color GradientBaseColor = Color.FromRgb(0xf0, 0x80, 0x90);
        static Color _passColor = Colors.Green;
        static Color[] _failColors = {
            Colors.Red,
            Colors.Orchid,
            Colors.Orange,
            Colors.OliveDrab,
            Colors.Olive,
            Colors.Navy,
            Colors.MediumVioletRed,
            Colors.MediumTurquoise,
            Colors.MediumSlateBlue,
            Colors.LightSkyBlue,
            Colors.LightSlateGray,
            Colors.LightSteelBlue,
            Colors.Maroon,
            Colors.MediumBlue,
            Colors.MediumOrchid,
            Colors.MediumPurple,
            Colors.Magenta,
            Colors.PaleVioletRed,
            Colors.SlateGray,
            Colors.SteelBlue,
            Colors.Tan,
            Colors.Teal,
            Colors.SlateBlue,
            Colors.Thistle,
            Colors.Turquoise,
            Colors.Violet,
            Colors.Tomato,
            Colors.LightSeaGreen,
            Colors.SkyBlue,
            Colors.Sienna,
            Colors.Peru,
            Colors.Pink,
            Colors.Plum,
            Colors.Purple,
            Colors.RoyalBlue,
            Colors.SaddleBrown,
            Colors.Salmon,
            Colors.SandyBrown,
            Colors.RosyBrown,
            Colors.Yellow,
            Colors.LightSalmon,
            Colors.DarkRed,
            Colors.DarkOrchid,
            Colors.DarkOrange,
            Colors.DarkOliveGreen,
            Colors.DarkMagenta,
            Colors.DarkKhaki,
            Colors.DarkGray,
            Colors.DarkGoldenrod,
            Colors.DarkCyan,
            Colors.DarkBlue,
            Colors.Cyan,
            Colors.Crimson,
            Colors.CornflowerBlue,
            Colors.Coral,
            Colors.Chocolate,
            Colors.Aqua,
            Colors.Aquamarine,
            Colors.DarkSalmon,
            Colors.Blue,
            Colors.BlueViolet,
            Colors.Brown,
            Colors.BurlyWood,
            Colors.CadetBlue,
            Colors.Chartreuse,
            Colors.DarkSeaGreen,
            Colors.DarkSlateBlue,
            Colors.DarkSlateGray,
            Colors.HotPink,
            Colors.IndianRed,
            Colors.Indigo,
            Colors.Khaki,
            Colors.Lavender,
            Colors.LightBlue,
            Colors.LightCoral,
            Colors.LightGray,
            Colors.LightPink,
            Colors.Gray,
            Colors.DarkTurquoise,
            Colors.DarkViolet,
            Colors.DeepPink,
            Colors.DeepSkyBlue,
            Colors.DimGray,
            Colors.DodgerBlue,
            Colors.Firebrick,
            Colors.Fuchsia,
            Colors.Gold,
            Colors.Goldenrod,
            Colors.PaleGoldenrod,
            Colors.Black
        };


        private static Color GetGradientColor(int level) {
            return Color.FromRgb((byte)(GradientBaseColor.R-level*7), (byte)(GradientBaseColor.G - level * 5), (byte)(GradientBaseColor.B - level * 5));
        }

        public static Color GetPassBinColor() {
            return _passColor;
        }
        public static void SetPassBinColor(Color color) {
            _passColor = color;
        }

        public static Color GetFailBinColor(int index) {
            if (index < 0) throw new Exception("Wrong Index");
            if (index >= _failColors.Length) return Colors.Black;
            return _failColors[index];
        }

        public static Color[] GetFailBinColors() {
            return _failColors;
        }
        public static void SetFailBinColors(Color[] colors) {
            _failColors = colors;
        }

        public static Color GetStackWaferBinColor(int failCnt, int totalStackCnt) {
            if (failCnt == 0) return _passColor;
            if (failCnt >= totalStackCnt) return GetGradientColor(_gradientCnt);

            return GetGradientColor(failCnt * _gradientCnt / totalStackCnt);
        }
    }
}
