using System;
using System.Linq;
using System.Windows;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public static class ExtensionMethods
    {
        public static string SpaceBetweenWords(this string str)
        {
            return string.Concat(str.Select((c, i) => i != 0 && char.IsUpper(c) ? " " + c : c.ToString()));
        }

        public static void HandleAsUserNotification(this Exception e)
        {
            // TODO: replace with better ProWindow-derived dialog
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}