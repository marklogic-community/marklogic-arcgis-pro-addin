using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MarkLogic.Client.Search
{
    public class SearchOptions
    {
        private XmlNamespaceManager nsManager;
        private XmlDocument doc;

        public SearchOptions(string xmlContent)
        {
            doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("search", "http://marklogic.com/appservices/search");
        }

        private List<Constraint> constraints = null;
        public IEnumerable<Constraint> Constraints
        {
            get
            {
                if (constraints == null)
                {
                    constraints = new List<Constraint>();
                    foreach (XmlNode node in doc.SelectNodes("search:options/search:constraint", nsManager))
                        constraints.Add(new Constraint(node, nsManager));
                }
                return constraints;
            }
        }
    }

    public class Constraint
    {
        private XmlNamespaceManager nsManager;
        private XmlNode node;

        public Constraint(XmlNode node, XmlNamespaceManager nsManager)
        {
            this.node = node;
            this.nsManager = nsManager;
        }

        public string Name
        {
            get
            {
                var nameAttrib = node.SelectSingleNode("@name");
                return nameAttrib?.Value;
            }
        }

        public bool IsFacet
        {
            get
            {
                var facetAttrib = node.SelectSingleNode("*/@facet");
                return facetAttrib != null ? (facetAttrib.Value.ToLower() == "true") : false;
            }
        }

        public string AnnotationDescription
        {
            get
            {
                var description = node.SelectSingleNode("search:annotation/description", nsManager);
                return description != null ? description.InnerText : null;
            }
        }
    }
}
