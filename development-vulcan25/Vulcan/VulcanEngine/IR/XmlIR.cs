using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using AstFramework;
using Vulcan.Utility.Collections;
using Vulcan.Utility.ComponentModel;
using VulcanEngine.Common;

namespace VulcanEngine.IR
{
    public class XmlIR : VulcanNotifyPropertyChanged, IIR, ICloneable
    {
        public const string VulcanPrefix = "rc";

        #region Private and Protected Storage

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
        }
        #endregion

        #region Public Accessors

        public Guid Id { get; protected set; }

        public string TemplatePath { get; set; }

        public VulcanCollection<BimlFile> BimlFiles { get; private set; }

        public XmlSchemaSet SchemaSet { get; set; }

        public string DefaultXmlNamespace { get; set; }

        public bool IsValidated { get; private set; }

        #endregion  // Public Accessors

        #region Initialization

        protected XmlIR(XmlIR xmlIR)
        {
            IsValidated = xmlIR.IsValidated;
            BimlFiles = xmlIR.BimlFiles;
            SchemaSet = xmlIR.SchemaSet;
            DefaultXmlNamespace = xmlIR.DefaultXmlNamespace;
            Id = xmlIR.Id;
            VulcanOnPropertyChanged("BimlFiles", null, BimlFiles);
        }

        public XmlIR()
        {
            SchemaSet = new XmlSchemaSet();
            SchemaSet.Add(null, XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("VulcanEngine.Content.xsd.vulcan2.xsd")));

            Id = Guid.NewGuid();
            IsValidated = false;
            BimlFiles = new VulcanCollection<BimlFile>();
            BimlFiles.CollectionChanged += _bimlFiles_CollectionChanged;
            VulcanOnPropertyChanged("BimlFiles", null, BimlFiles);
        }

        #endregion  // Initialization

        private void _bimlFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            VulcanOnCollectionPropertyChanged("BimlFiles", e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "Required for parsing infrastructure.")]
        public BimlFile AddXml(string fileName, XmlDocument xmlDocument, XmlIRDocumentType docType, bool isReadOnly)
        {
            BimlFile bimlFile = AddXml(fileName, docType, isReadOnly);
            bimlFile.Text = xmlDocument.OuterXml;
            return bimlFile;
        }

        public BimlFile AddXml(string fileName, XmlIRDocumentType docType, bool isReadOnly)
        {
            var bimlFile = new BimlFile(docType, fileName, isReadOnly);
            return AddXml(bimlFile);
        }

        public BimlFile AddXml(BimlFile bimlFile)
        {
            if (!BimlFiles.Any(item => item.FilePath == bimlFile.FilePath))
            {
                BimlFiles.Add(bimlFile);
            }

            return bimlFile;
        }

        #region XDocument Validation
        private BimlFile _currentBimlFile;

        public void ValidateXDocuments()
        {
            foreach (BimlFile bimlFile in BimlFiles)
            {
                _currentBimlFile = bimlFile;
                if (bimlFile.XDocument == null)
                {
                    try
                    {
                        XDocument.Load(new StringReader(bimlFile.Text), LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
                    }
                    catch (XmlException e)
                    {
                        MessageEngine.Trace(bimlFile.FilePath, e.LineNumber, e.LinePosition, Severity.Error, "V0150", e, e.Message);
                    }
                }
                else
                {
                    bimlFile.XDocument.Validate(SchemaSet, ValidationEventHandler, false);
                }
            }

            IsValidated = true;
            Id = Guid.NewGuid();
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            var xmlLineInfo = sender as IXmlLineInfo;
            string line = "?";
            string offset = "?";
            if (xmlLineInfo != null)
            {
                line = xmlLineInfo.LineNumber.ToString(CultureInfo.InvariantCulture);
                offset = xmlLineInfo.LinePosition.ToString(CultureInfo.InvariantCulture);
            }

            string fileName = _currentBimlFile.Name;

            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    MessageEngine.Trace(Severity.Error, "{0}({1},{2}): error V0102: {3}", fileName, line, offset, e.Message);
                    break;
                case XmlSeverityType.Warning:
                    MessageEngine.Trace(Severity.Warning, "{0}({1},{2}): warning V0102: {3}", fileName, line, offset, e.Message);
                    break;
            }
        }
        #endregion  // XDocument Validation
    }
}
