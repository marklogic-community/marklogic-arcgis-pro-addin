using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Document
{
    public class DocumentService
    {
        private static DocumentService _instance;

        private DocumentService()
        {
        }

        public static DocumentService Instance => _instance ?? (_instance = new DocumentService());

        public async Task<Document> Fetch(Connection connection, string documentUri, string transform)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = "v1/documents" };
            ub.AddQueryParam("uri", documentUri);
            ub.AddQueryParam("transform", transform);
            using (var msg = new HttpRequestMessage(HttpMethod.Get, ub.Uri))
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new Document(documentUri, responseContent);
                }
            }
        }
    }
}
