using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for InspectResultsWindow.xaml
    /// </summary>
    public partial class InspectResultsWindow : ProWindow
    {
        public InspectResultsWindow()
        {
            InitializeComponent();
        }

        private void ProWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as InspectResultsViewModel;
            Debug.Assert(viewModel != null);
            if (viewModel == null)
                return;

            ProgressDialog pDlg = new ProgressDialog("Retrieving items...");
            pDlg.Show();
            viewModel.Search.Execute(null);
            pDlg.Hide();
            pDlg.Dispose();
        }
    }
}
