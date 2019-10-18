using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MarkLogic
{
    public class ConnectionProfile : ICloneable
    {
        private string _name = null;

        public ConnectionProfile()
        {
        }

        [XmlElement]
        public string Name
        {
            get
            {
                if (_name != null)
                    return _name;
                else if (Uri != null)
                    return Uri.PathAndQuery;
                else
                    return null;
            }
            set { _name = value; }
        }

        [XmlElement]
        public string Host { get; set; }

        [XmlElement]
        public int Port { get; set; }

        [XmlElement]
        public bool IsSSL { get; set; }

        [XmlIgnore]
        public Uri Uri
        {
            get
            {
                try
                {
                    var ub = new UriBuilder() { Host = Host, Port = Port, Scheme = IsSSL ? Uri.UriSchemeHttps : Uri.UriSchemeHttp };
                    return ub.Uri;
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }

        public object Clone()
        {
            var clone = new ConnectionProfile() { Host = Host, Port = Port, IsSSL = IsSSL };
            if (_name != null)
                clone.Name = _name;
            return clone;
        }
    }
}
