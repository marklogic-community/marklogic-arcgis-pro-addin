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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Settings
{
    /// <summary>
    /// Interaction logic for ServersView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            var connProfile = new ConnectionProfile();
            var dlg = new ServerDetailsWindow();
            dlg.DataContext = connProfile;
            var dlgResult = dlg.ShowDialog();
            dlg.Close();
            if (dlgResult.HasValue && dlgResult.Value == true && DataContext != null && DataContext is SettingsViewModel)
            {
                var viewModel = (SettingsViewModel)DataContext;
                if (viewModel.AddServerCommand.CanExecute(null))
                    viewModel.AddServerCommand.Execute(connProfile);
            }
        }

        private void EditServer_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(gridServers.SelectedItem != null);
            if (gridServers.SelectedItem == null)
                return;
            Debug.Assert(gridServers.SelectedItem is ConnectionProfile);
            if (!(gridServers.SelectedItem is ConnectionProfile))
                return;
            var connProfile = (ConnectionProfile)gridServers.SelectedItem;

            var dlg = new ServerDetailsWindow();
            dlg.DataContext = connProfile;
            var dlgResult = dlg.ShowDialog();
            dlg.Close();
            if (dlgResult.HasValue && dlgResult.Value == true && DataContext != null && DataContext is SettingsViewModel)
            {
                var viewModel = (SettingsViewModel)DataContext;
                if (viewModel.EditServerCommand.CanExecute(null))
                    viewModel.EditServerCommand.Execute(connProfile);
            }
        }

        private void DeleteServer_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(gridServers.SelectedItem != null);
            if (gridServers.SelectedItem == null)
                return;
            var delete = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Delete the selected server?", "MarkLogic", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (delete && DataContext != null && DataContext is SettingsViewModel)
            {
                var viewModel = (SettingsViewModel)DataContext;
                if (viewModel.DeleteServerCommand.CanExecute(null))
                    viewModel.DeleteServerCommand.Execute(gridServers.SelectedItem);
            }
        }
    }
}
