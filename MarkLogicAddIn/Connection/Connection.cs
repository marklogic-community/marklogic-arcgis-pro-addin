using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarkLogic
{
    public class Connection
    {
        private ConnectionProfile _profile;
        private HttpClient _http;
        
        internal Connection(ConnectionProfile profile, HttpClient http)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            _profile = (ConnectionProfile)profile.Clone();
            _http = http ?? throw new ArgumentNullException("http");
        }

        public ConnectionProfile Profile => _profile;
        
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = await _http.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var wwwAuth = response.Headers.WwwAuthenticate.FirstOrDefault();
                var scheme = wwwAuth != null ? wwwAuth.Scheme : "Basic";
                var domain = scheme.ToLower() == "digest" ? wwwAuth.GetWwwAuthParameterValue("realm") : null;
                throw new AuthorizationRequiredException(new AuthorizationRequiredInfo(request.RequestUri.ToString(), scheme, domain));
            }
            response.EnsureSuccessStatusCode();
            return response;
        }
    }

    public class AuthorizationRequiredException : Exception
    {
        public AuthorizationRequiredException(AuthorizationRequiredInfo info)
            : base($"Authorization required for '{info.RequestUri}'.")
        {
            AuthInfo = info;
        }

        public AuthorizationRequiredInfo AuthInfo { get; private set; }
    }

    public class AuthorizationRequiredInfo
    {
        public AuthorizationRequiredInfo(string requestUri, string scheme, string domain)
        {
            RequestUri = requestUri;
            Scheme = scheme;
            Domain = domain;
        }

        public string RequestUri { get; private set; }

        public string Scheme { get; private set; }

        public string Domain { get; private set; }
    }
}
