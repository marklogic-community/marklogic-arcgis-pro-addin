using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarkLogic
{
    public class Connection
    {
        private HttpClient _http;
        
        internal Connection(ConnectionProfile profile, HttpClient http)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            Profile = (ConnectionProfile)profile.Clone();
            _http = http ?? throw new ArgumentNullException("http");
        }

        public ConnectionProfile Profile { get; private set; }
        
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = await _http.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var wwwAuth = response.Headers.WwwAuthenticate.FirstOrDefault();
                var scheme = wwwAuth != null ? wwwAuth.Scheme : "Basic";
                var domain = scheme.ToLower() == "digest" ? wwwAuth.GetWwwAuthParameterValue("realm") : null;
                throw new AuthorizationRequiredException(Profile, request.RequestUri.ToString(), scheme, domain);
            }
            response.EnsureSuccessStatusCode();
            return response;
        }
    }

    public class AuthorizationRequiredException : Exception
    {
        public AuthorizationRequiredException(ConnectionProfile connProfile, string requestUri, string scheme, string domain)
            : base($"Authorization required for '{requestUri}'.")
        {
            ConnectionProfile = connProfile;
            RequestUri = requestUri;
            Scheme = scheme;
            Domain = domain;
        }

        public ConnectionProfile ConnectionProfile { get; private set; }

        public string RequestUri { get; private set; }

        public string Scheme { get; private set; }

        public string Domain { get; private set; }
    }
}
