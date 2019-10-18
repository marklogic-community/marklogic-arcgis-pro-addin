using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Document
{
    public class Document
    {
        public Document(string documentUri, string responseContent)
        {
            DocumentUri = documentUri;
            RawContent = responseContent;
        }

        public string DocumentUri { get; private set; }

        public string RawContent { get; private set; }
    }
}
