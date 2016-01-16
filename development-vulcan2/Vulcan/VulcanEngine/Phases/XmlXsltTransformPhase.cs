using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Reflection;
using System.IO;

using VulcanEngine.Properties;
using VulcanEngine.IR;
using VulcanEngine.Common;

namespace VulcanEngine.Phases
{
    // REVIEW: Do we need to store friendy names in resources?  Overkill perhaps?  They'll never be translated.
    [PhaseFriendlyName("XmlXsltTransformPhase")]
    public class XmlXsltTransformPhase : IPhase
    {
        private MessageEngine _message;
        private string _workflowUniqueName;

        public XmlXsltTransformPhase(string WorkflowUniqueName)
        {
            this._workflowUniqueName = WorkflowUniqueName;
            this._message = MessageEngine.Create(this.WorkflowUniqueName);
        }

        #region IPhase Members

        public string Name
        {
            get { return "XmlXsltTransformPhase"; }
        }

        public string WorkflowUniqueName
        {
            get { return this._workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(XmlIR); }
        }

        public VulcanEngine.IR.IIR Execute(List<IIR> PredecessorIRs)
        {
            return Execute(IRUtility.MergeIRList(this.Name, PredecessorIRs));
        }

        public VulcanEngine.IR.IIR Execute(IIR PredecessorIR)
        {
            XmlIR xmlIR = PredecessorIR as XmlIR;
            if (xmlIR == null)
            {
                _message.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, PredecessorIR.GetType().ToString(), this.Name);
            }

            string XsltFolderPath = PathManager.GetToolSubpath(Settings.Default.SubPathXsltTransformFolder);
            string[] XsltFileNames = Directory.GetFiles(XsltFolderPath, "*.xsl");

            if (XsltFileNames.Length <= 0)
            {
                _message.Trace(Severity.Warning, Resources.WarningNoPreProcessorFound);
                return null;
            }

            XsltSettings settings = new XsltSettings(true, false);
            XmlIR output = new XmlIR();

            // REVIEW: The approach I take here is to pipeline XSLT transforms using a MemoryStream.  This isn't great.
            //         In the next .NET Fx, they are expecting to fix XslCompiledTransform so it can pipeline more resonably.  This should be changed at that time.
            foreach (XmlIRDocumentType docType in xmlIR.XDocuments.Keys)
            {
                foreach (XDocument xDocument in xmlIR.XDocuments[docType])
                {
                    XmlDocument intermediateXMLDocument = new XmlDocument();
                    intermediateXMLDocument.Load(xDocument.CreateReader());
                    foreach (string s in XsltFileNames)
                    {
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        XsltArgumentList args = new XsltArgumentList();
                        args.AddParam("XSLTFolderPath", String.Empty, XsltFolderPath);

                        xslt.Load(s, settings, new XmlUrlResolver());

                        MemoryStream intermediateMemoryStream = new MemoryStream();
                        xslt.Transform(intermediateXMLDocument, args, intermediateMemoryStream);

                        intermediateMemoryStream.Position = 0;

                        intermediateXMLDocument = new XmlDocument();
                        intermediateXMLDocument.Load(intermediateMemoryStream);
                    }
                    output.AddXml(intermediateXMLDocument,docType);
                }
            }
            return output;   
        }
        #endregion
    }
}
