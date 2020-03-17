using System;
using System.Windows;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public static class ExtensionMethods
    {
        public static void HandleAsUserNotification(this Exception e)
        {
            // TODO: replace with better ProWindow-derived dialog
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}