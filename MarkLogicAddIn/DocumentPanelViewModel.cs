using ArcGIS.Desktop.Framework;
using MarkLogic.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class DocumentPanelViewModel : ViewModelBase
    {
        private const string _noDocHtml = @"
        <html>
            <body>
                <div style=""font-family:Arial;padding-top:100px;text-align:center"">
                No document selected.
                </div>
            </body>
        </html>";

        public DocumentPanelViewModel()
        {
            Reset();
        }

        private bool _isFetching;
        public bool IsFetching
        {
            get { return _isFetching; }
            set { SetProperty(ref _isFetching, value); }
        }

        private string _documentUri;
        public string DocumentUri
        {
            get { return _documentUri; }
            set { SetProperty(ref _documentUri, value); }
        }

        private string _formattedContent;
        public string FormattedContent
        {
            get { return _formattedContent; }
            set { SetProperty(ref _formattedContent, value); }
        }

        public void Reset()
        {
            DocumentUri = null;
            FormattedContent = _noDocHtml;
        }

        public async void FetchDocument(ConnectionProfile connProfile, string documentUri, string transform)
        {
            try
            {
                IsFetching = true;
                var conn = ConnectionService.Instance.Create(connProfile);

                var document = await DocumentService.Instance.Fetch(conn, documentUri, transform);

                DocumentUri = documentUri;
                FormattedContent = document.RawContent;
            }
            catch (AuthorizationRequiredException e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsFetching = false;
            }
        }
    }
}
