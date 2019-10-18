using ArcGIS.Desktop.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Settings
{
    /// <summary>
    /// Interaction logic for ServerDetailsWindow.xaml
    /// </summary>
    public partial class ServerDetailsWindow : ProWindow
    {
        public ServerDetailsWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is ConnectionProfile)
            {
                string validationMsg = null;
                var connProfile = (ConnectionProfile)DataContext;
                if (string.IsNullOrWhiteSpace(connProfile.Name))
                    validationMsg = "Name is required.";
                else if (connProfile.Uri == null)
                    validationMsg = "Invalid or incorrect host.";

                if (!string.IsNullOrWhiteSpace(validationMsg))
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(validationMsg, "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
