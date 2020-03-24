using ArcGIS.Desktop.Framework.Controls;
using System.Security;
using System.Windows;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Controls
{
    public partial class CredentialsWindow : ProWindow
    {
        public CredentialsWindow()
        {
            InitializeComponent();
        }

        public ConnectionProfile ConnectionProfile { get; set; }

        public string Username { get; set; }

        public SecureString Password { get; set; }

        private void ProWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtCredentialsFor.Text = ConnectionProfile?.Uri.ToString();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Username = inputUsername.Text;
            Password = inputPassword.SecurePassword;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
