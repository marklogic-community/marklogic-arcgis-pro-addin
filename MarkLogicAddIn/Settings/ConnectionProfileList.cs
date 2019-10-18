using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Settings
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ConnectionProfileList
    {
        public ConnectionProfileList()
        {
            Items = new ConnectionProfile[0];
        }

        [XmlArray]
        public ConnectionProfile[] Items { get; set; }
    }
}
