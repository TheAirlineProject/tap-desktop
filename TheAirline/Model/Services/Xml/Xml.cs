using System.Collections.Generic;
using TheAirline.Model.Services.Filesystem;
using System.Xml;

namespace TheAirline.Model.Services.Xml
{
    public class Xml
    {
        private File xmlFile;
        private XmlDocument xmlDoc;
        private List<XmlElement> elements = new List<XmlElement>();

        public Xml(File file = null)
        {
            this.xmlFile = file;
        }

        public void SetFile(File file)
        {
            this.xmlFile = file;
        }

        public void Initialize()
        {
            this.xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(xmlDeclaration);
        }

        public void Append(XmlNode child)
        {
            if (elements.Count > 0)
            {
                
            }
            xmlDoc.AppendChild(child);
        }

        public void Comment(string comment, XmlNode parent = null)
        {
            XmlComment xmlComment = xmlDoc.CreateComment(comment);
            XmlNode appendTo = parent ?? xmlDoc.FirstChild;
            appendTo.AppendChild(xmlComment);
        }

        public void Save()
        {
            xmlDoc.Save(xmlFile.ToString());
        }

        public XmlElement CreateElement(string name, XmlElement parent = null)
        {
            XmlNode appendTo = xmlDoc.FirstChild;

            if (parent != null)
            {
                appendTo = elements.Find(p => (p.BaseURI == parent.BaseURI));
            }

            XmlElement el = xmlDoc.CreateElement(name);
            appendTo.AppendChild(el);
            elements.Add(el);
            return el;
        }
    }
}
