using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkLogic
{
    public static class ExtensionMethods
    {
        public static async Task<HttpRequestMessage> CloneMessage(this HttpRequestMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");
            var clone = new HttpRequestMessage(message.Method, message.RequestUri) { Version = message.Version };
            clone.Properties.ToList().ForEach(p => clone.Properties.Add(p.Key, p.Value));
            clone.Headers.ToList().ForEach(h => clone.Headers.TryAddWithoutValidation(h.Key, h.Value));
            if (message.Content != null)
            {
                var buffer = new MemoryStream();
                await message.Content.CopyToAsync(buffer);
                buffer.Position = 0;
                clone.Content = new StreamContent(buffer);

                if (message.Content.Headers != null)
                    message.Content.Headers.ToList().ForEach(h => clone.Content.Headers.Add(h.Key, h.Value));
            }
            return clone;
        }

        public static UriBuilder AddQueryParam(this UriBuilder uriBuilder, string paramName, object value)
        {
            if (uriBuilder == null) throw new ArgumentNullException("uriBuilder");
            if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentNullException("paramName");
            var valueString = value != null ? value.ToString() : "";
            var qp = $"{paramName}={valueString}";
            uriBuilder.Query = string.IsNullOrWhiteSpace(uriBuilder.Query) ? qp : uriBuilder.Query.TrimStart('?') + "&" + qp;
            return uriBuilder;
        }

        public static string GetWwwAuthParameterValue(this AuthenticationHeaderValue wwwAuth, string key)
        {
            var match = Regex.Match(wwwAuth.Parameter, $"{key}=\"(?<key>\\w*)\"");
            return match.Success ? match.Groups["key"].Value : null;
        }
    }
}
