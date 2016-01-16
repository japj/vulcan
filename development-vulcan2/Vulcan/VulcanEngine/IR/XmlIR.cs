using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.IO;

using VulcanEngine.Common;
using VulcanEngine.Properties;

namespace VulcanEngine.IR
{
    public enum XmlIRDocumentType
    {
        SOURCE,
        INCLUDE
    }

    public class XmlIR : IIR, ICloneable
    {
        public static string VULCAN_PREFIX = "rc";

        #region Private and Protected Storage
        private Dictionary<XmlIRDocumentType,List<XDocument>> _documents;
        private HashSet<string> _documentsLoaded;
        private static Dictionary<string, string> _documentRoots = new Dictionary<string, string>();
        private XmlSchemaSet _xmlSchemaSet;
        private bool _isValidated;
        protected Guid _id;
        private MessageEngine _message;
        #endregion  // Private and Protected Storage

        #region IIR Members
        public virtual string Name
        {
            get { return "XmlIR"; }
        }
        #endregion

        #region ICloneable Members
        public virtual object Clone()
        {
            throw new NotSupportedException("XmlIR cannot currently be cloned.  It would result in the loss of validation and annotation data.");
            // TODO: This is broken - it doesn't preserve XDocument annotations
/*            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(this.XDocument.CreateReader());
            XmlNode clonedXmlDoc = xmlDoc.Clone();

            XDocument cloned = XDocument.Load(new XmlNodeReader(clonedXmlDoc));

            XmlIR xmlIR = new XmlIR(cloned);
            if (this.IsValidated)
            {
                xmlIR.ValidateXDocument();
            }

            return xmlIR;*/
        }
        #endregion

        #region Public Accessors
        public Guid ID
        {
            get { return _id; }
        }

        public IDictionary<XmlIRDocumentType, List<XDocument>> XDocuments
        {
            get { return _documents; }
        }
        public IList<XDocument> AllXDocuments
        {

            get
            {
                List<XDocument> docList = new List<XDocument>();

                foreach (XmlIRDocumentType docType in _documents.Keys)
                {
                    docList.AddRange(_documents[docType]);
                }
                return docList;
            }
        }

        public XmlSchemaSet SchemaSet
        {
            get { return this._xmlSchemaSet; }
            set { this._xmlSchemaSet = value; }
        }

        public bool IsValidated
        {
            get { return _isValidated; }
        }
        #endregion  // Public Accessors

        #region Initialization

        protected XmlIR(XmlIR xmlIR)
        {
            this._isValidated = xmlIR._isValidated;
            this._documents = xmlIR._documents;
            this._xmlSchemaSet = xmlIR._xmlSchemaSet;
            this._id = xmlIR._id;
            this._message = xmlIR._message;
            this._documentsLoaded = xmlIR._documentsLoaded;
        }

        public XmlIR()
        {
            // TODO: Cleanup up schema handling - put it all in a configured location
            _xmlSchemaSet = new XmlSchemaSet();
            _xmlSchemaSet.Add(null, PathManager.GetToolSubpath("xsd\\vulcan2.xsd"));

            this._documents = new Dictionary<XmlIRDocumentType, List<XDocument>>();
            this._documentsLoaded = new HashSet<string>();

            foreach (XmlIRDocumentType docType in Enum.GetValues(typeof(XmlIRDocumentType)))
            {
                this._documents.Add(docType, new List<XDocument>());
            }

            this._id = Guid.NewGuid();
            this._isValidated = false;
            this._message = MessageEngine.Create(String.Format("__XmlIR: {0}", this._id));
        }
        #endregion  // Initialization

        public void SetDocumentRoot(string rootName, string rootPath)
        {
            if (!_documentRoots.ContainsKey(rootName))
            {
                _documentRoots.Add(rootName, rootPath);
            }
        }
        
        public void AddXml(XmlDocument xmlDocument, XmlIRDocumentType docType)
        {
            AddXml(
                XDocument.Load(XmlTextReader.Create(new System.IO.StringReader(xmlDocument.OuterXml))),
                docType
                );
        }

        
        public XDocument AddXml(string fileName, XmlIRDocumentType docType)
        {
            bool fileNameExpanded = false;
            string rootName = fileName.Split(new char[] { '.' })[0];
            if (_documentRoots.ContainsKey(rootName))
            {
                string fileNameTailing = fileName.Substring(rootName.Length + 1);
                fileName = Path.Combine(_documentRoots[rootName], fileNameTailing.Replace('.', '\\')) + ".xml";
                fileNameExpanded = true;
            }

            if (Path.IsPathRooted(fileName))
            {
                if (File.Exists(fileName))
                {
                    if (!_documentsLoaded.Contains(fileName))
                    {
                        XDocument xDocument = XDocument.Load(fileName);
                        AddXml(xDocument, docType);

                        if (fileNameExpanded)
                        {
                            xDocument.Validate(SchemaSet, this.ValidationEventHandler, true);
                        }

                        _documentsLoaded.Add(fileName);

                        return xDocument;
                    }
                }
                else
                {
                    MessageEngine.Global.Trace(Severity.Warning, "<Using> file not found: {0}", fileName);
                }
            }

            return null;
        }

        public void AddXml(XDocument xDocument, XmlIRDocumentType docType)
        {
            _documents[docType].Add(xDocument);
        }

        #region XDocument Validation
        public void ValidateXDocuments()
        {
            foreach (XmlIRDocumentType docType in this._documents.Keys)
            {
                foreach (XDocument xDocument in this._documents[docType])
                {
                    xDocument.Validate(SchemaSet, this.ValidationEventHandler, true);
                }
            }
            this._isValidated = true;
            this._id = Guid.NewGuid();
        }

        public void ValidateXElement(XmlSchemaObject schemaObject, XElement xElement)
        {
            xElement.Validate(schemaObject, SchemaSet, this.ValidationEventHandler, true);
        }

        protected void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    _message.Trace(Severity.Error, Resources.ErrorXmlValidation, e.Message);
                    break;
                case XmlSeverityType.Warning:
                    _message.Trace(Severity.Warning, Resources.WarningXmlValidation, e.Message);
                    break;
            }
        }
        #endregion  // XDocument Validation
    }
}
