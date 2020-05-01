using MarkLogic.Client.Document;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class DocumentViewModel : ViewModelBase
    {
        private const string NoDocHTML = @"
        <html>
            <body>
                <div style=""font-family:Arial;padding-top:100px;text-align:center"">
                No document selected.
                </div>
            </body>
        </html>";

        public DocumentViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ViewDocumentMessage>(m => ViewDocument(m.DocumentUri));
            MessageBus.Subscribe<ServerSettingsChangedMessage>(m => Reset());
            MessageBus.Subscribe<BeginSearchMessage>(m => Reset());
            Reset();
        }

        protected MessageBus MessageBus { get; private set; }

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
            FormattedContent = NoDocHTML;
        }

        private async Task ViewDocument(string documentUri)
        {
            try
            {
                IsFetching = true;

                var serverMsg = new GetServerSettingsMessage();
                await MessageBus.Publish(serverMsg);
                Debug.Assert(serverMsg.Resolved);
                if (!serverMsg.Resolved)
                {
                    Reset();
                    return;
                }

                var conn = ConnectionService.Instance.Create(serverMsg.Profile);
                var document = await DocumentService.Instance.Fetch(conn, documentUri, serverMsg.ServiceModel.DocTransform);

                DocumentUri = documentUri;
                FormattedContent = document.RawContent;
            }
            catch (Exception e)
            {
                throw e; // let ServerCommand handle it
            }
            finally
            {
                IsFetching = false;
            }
        }
    }
}
