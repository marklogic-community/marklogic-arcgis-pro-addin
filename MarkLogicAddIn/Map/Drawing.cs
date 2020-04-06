using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public static class Drawing
    {
        private static int GetSystemProperty(string propName, int defaultValue)
        {
            var prop = typeof(SystemParameters).GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null)
            {
                Debug.WriteLine($"Unable to get system property {propName}; returning default value of {defaultValue}.");
                return defaultValue;
            }
            else
                return (int)prop.GetValue(null);
        }

        public const int Ppi = 72; // points per inch

        public const int DefaultDPI = 96; // dots per inch

        private static Lazy<int> _dpiX = new Lazy<int>(() => GetSystemProperty("DpiX", DefaultDPI));
        public static int DpiX => _dpiX.Value;

        private static Lazy<int> _dpi = new Lazy<int>(() => GetSystemProperty("Dpi", DefaultDPI));
        public static int Dpi => _dpi.Value;

        public static int DpiY => Dpi;

        public static double PixelsFromPoints(double points) => (points * Dpi) / Ppi;

        public static Color Negative(Color color) => Color.Subtract(Colors.White, color);
    }
}
