using System;
using System.IO;
using System.Security.Permissions;
using System.Xml.Linq;
using Vulcan.Utility.Files;

namespace AstFramework
{
    public enum XmlIRDocumentType
    {
        Source,
        Include
    }

    public enum BimlFileTextChangedSource
    {
        RawText,
        XDocument,
    }

    public class BimlFile : WatchableFile, INamedFile
    {
        public event EventHandler<BimlFileTextChangedEventArgs> TextChanged;

        protected void OnTextChanged(BimlFileTextChangedSource bimlFileTextChangedSource)
        {
            if (TextChanged != null)
            {
                TextChanged(this, new BimlFileTextChangedEventArgs(bimlFileTextChangedSource));
            }
        }

        public event EventHandler<EventArgs> ParseableChanged;

        protected void OnParseableChanged()
        {
            if (ParseableChanged != null)
            {
                ParseableChanged(this, new EventArgs());
            }
        }

        private XmlIRDocumentType _emitType;
        private string _name;
        private string _path;
        private bool _dirty;
        private bool _documentLoadable;
        private bool _parseable;
        private bool _documentChanging;
        private bool _textChanging;
        private string _text;
        private XDocument _document;

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public BimlFile(XmlIRDocumentType type, string path, bool bimlFileMember)
            : this(type, path, bimlFileMember, new XDocument(new XElement(XName.Get("Vulcan", "http://tempuri.org/vulcan2.xsd"))))
        {
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public BimlFile(XmlIRDocumentType type, string path, bool bimlFileMember, bool isReadOnly)
            : this(type, path, bimlFileMember, new XDocument(new XElement(XName.Get("Vulcan", "http://tempuri.org/vulcan2.xsd"))), isReadOnly)
        {
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public BimlFile(XmlIRDocumentType type, string path, bool bimlFileMember, XDocument document)
            : this(type, path, bimlFileMember, document, false)
        {
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public BimlFile(XmlIRDocumentType type, string path, bool bimlFileMember, XDocument document, bool isReadOnly)
            : base(path, isReadOnly)
        {
            IsInXmlIR = bimlFileMember;

            _emitType = type;
            FilePath = path;

            if (isReadOnly)
            {
                XDocument = XDocument.Load(FilePath, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
            }
            else
            {
                if (document != null)
                {
                    Load(document);
                }
                else
                {
                    throw new NullReferenceException("Error: BimlFile: XDocument should not be null if isReadOnly is set to false.");
                }
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public BimlFile(XmlIRDocumentType type, string path)
            : this(type, path, true)
        {
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public BimlFile(XmlIRDocumentType type)
            : this(type, null)
        {
        }

        public XElement RootXElement { get; set; }

        private void Load(XDocument defaultXDocument)
        {
            if (File.Exists(_path))
            {
                Text = File.ReadAllText(_path);
                IsDirty = false;
            }
            else
            {
                Text = defaultXDocument.ToString();
            }
        }

        public void Save()
        {
            if (!String.IsNullOrEmpty(_path))
            {
                IsSaving = true;
                File.WriteAllText(_path, Text);
                IsDirty = false;
            }
        }

        private void MasterXDocument_Changed(object sender, XObjectChangeEventArgs e)
        {
            // TODO: Do we need to check if Text is dirty?  Not like we could revert the XDocumentChange
            if (!_textChanging)
            {
                _documentChanging = true;
                Text = XDocument.ToString();
                _documentChanging = false;
            }
        }

        public bool IsXDocumentLoadable
        {
            get { return _documentLoadable; }
            private set
            {
                if (!_documentLoadable.Equals(value))
                {
                    bool oldValue = _documentLoadable;
                    _documentLoadable = value;
                    VulcanOnPropertyChanged("IsXDocumentLoadable", oldValue, _documentLoadable);
                    if (!_documentLoadable)
                    {
                        IsParseable = false;
                    }
                }
            }
        }

        public bool IsParseable
        {
            get { return _parseable; }
            set
            {
                if (!_parseable.Equals(value))
                {
                    bool oldValue = _parseable;
                    _parseable = value;
                    VulcanOnPropertyChanged("IsParseable", oldValue, _parseable);
                    OnParseableChanged();
                }
            }
        }

        public XDocument XDocument
        {
            get { return _document; }
            private set
            {
                if (_document != value)
                {
                    var oldXDocument = _document;
                    if (_document != null)
                    {
                        _document.Changed -= MasterXDocument_Changed;
                    }

                    _document = value;
                    if (_document != null)
                    {
                        _document.Changed += MasterXDocument_Changed;
                    }

                    VulcanOnPropertyChanged("XDocument", oldXDocument, _document);
                }
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _textChanging = true;
                if (_text != value)
                {
                    IsDirty = true;

                    string oldText = _text;
                    _text = value;

                    AttemptLoadXDocumentFromText();
                    VulcanOnPropertyChanged("Text", oldText, _text);
                    OnTextChanged(_documentChanging ? BimlFileTextChangedSource.XDocument : BimlFileTextChangedSource.RawText);
                }

                _textChanging = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        private void AttemptLoadXDocumentFromText()
        {
            try
            {
                if (!_documentChanging || XDocument == null)
                {
                    var xdocument = XDocument.Parse(Text, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
                    XDocument = xdocument;
                    IsXDocumentLoadable = true;
                }
            }
            catch
            {
                IsXDocumentLoadable = false;
            }
        }

        public bool IsDirty
        {
            get { return _dirty; }
            set
            {
                if (_dirty != value)
                {
                    bool oldValue = _dirty;
                    _dirty = value;
                    VulcanOnPropertyChanged("IsDirty", oldValue, value);
                }
            }
        }

        public bool IsInXmlIR { get; private set; }

        public string Name
        {
            get { return _name; }
            set
            {
                string oldName = _name;
                string oldPath = _path;
                _name = value;
                _path = Path.Combine(Path.GetDirectoryName(_path), _name);
                VulcanOnPropertyChanged("Name", oldName, _name);
                VulcanOnPropertyChanged("FilePath", oldPath, _path);
            }
        }

        public string FilePath
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    string oldName = _name;
                    string oldPath = _path;

                    _path = value;
                    _name = Path.GetFileName(_path);

                    VulcanOnPropertyChanged("Name", oldName, _name);
                    VulcanOnPropertyChanged("FilePath", oldPath, _path);
                }
            }
        }

        public XmlIRDocumentType EmitType
        {
            get { return _emitType; }
            set
            {
                if (_emitType != value)
                {
                    XmlIRDocumentType oldValue = _emitType;
                    _emitType = value;
                    VulcanOnPropertyChanged("EmitType", oldValue, value);
                }
            }
        }
    }
}
