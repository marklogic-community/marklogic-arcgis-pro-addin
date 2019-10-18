using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic
{
    public class ConnectionService
    {
        private static ConnectionService _instance;

        private CredentialCache _credentialCache;

        private ConnectionService()
        {
            _credentialCache = new CredentialCache();
        }

        public static ConnectionService Instance => _instance ?? (_instance = new ConnectionService());

        public Connection Create(ConnectionProfile profile)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            
            var clientHandler = new HttpClientHandler();
            clientHandler.Credentials = _credentialCache;

            var http = new HttpClient(clientHandler) { BaseAddress = profile.Uri };
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            var conn = new Connection(profile, http);
            return conn;
        }

        public void SetCredentials(ConnectionProfile profile, string username, SecureString password, string authType, string domain)
        {
            SetCredentials(profile, new NetworkCredential(username, password, domain), authType);
        }

        public void SetCredentials(ConnectionProfile profile, NetworkCredential credentials, string authType)
        {
            var creds = _credentialCache.GetCredential(profile.Uri, authType);
            if (creds != null)
                _credentialCache.Remove(profile.Uri, authType);
            _credentialCache.Add(profile.Uri, authType, credentials);
        }
    }
}
