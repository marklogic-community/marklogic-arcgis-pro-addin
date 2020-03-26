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

namespace MarkLogic.Esri.ArcGISPro.AddIn.Controls
{
    /// <summary>
    /// Interaction logic for SaveSearchWindow.xaml
    /// </summary>
    public partial class SaveSearchWindow : ProWindow
    {
        public SaveSearchWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            var button = sender as Button;
            button.Command?.Execute(button.CommandParameter);
        }
    }
}
