using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using AstFramework;
using Vulcan.Utility.Files;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.Properties;

namespace VulcanEngine.Phases
{
    // REVIEW: Do we need to store friendy names in resources?  Overkill perhaps?  They'll never be translated.
    [PhaseFriendlyName("XmlXsltTransformPhase")]
    public class XmlXsltTransformPhase : IPhase
    {
        private readonly string _workflowUniqueName;
        private readonly string _xsltFiles;

        public XmlXsltTransformPhase(string workflowUniqueName, string xsltFiles)
        {
            _workflowUniqueName = workflowUniqueName;
            _xsltFiles = xsltFiles;
        }

        #region IPhase Members

        public string Name
        {
            get { return "XmlXsltTransformPhase"; }
        }

        public string WorkflowUniqueName
        {
            get { return _workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(XmlIR); }
        }

        public IIR Execute(Collection<IIR> predecessorIRs)
        {
            return Execute(IRUtility.MergeIRList(Name, predecessorIRs));
        }

        public IIR Execute(IIR predecessorIR)
        {
            var xmlIR = predecessorIR as XmlIR;
            if (xmlIR == null)
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, predecessorIR.GetType().ToString(), Name);
            }

            var xsltFileList = new List<string>();
            string xsltFolderPath = PathManager.GetToolSubpath(Settings.Default.SubPathXsltTransformFolder);
            foreach (string xsltFile in _xsltFiles.Split(';'))
            {
                if (!String.IsNullOrEmpty(xsltFile))
                {
                    xsltFileList.Add(Path.Combine(xsltFolderPath, xsltFile));
                }
            }

            var settings = new XsltSettings(true, false);
            var output = new XmlIR();

            // REVIEW: The approach I take here is to pipeline XSLT transforms using a MemoryStream.  This isn't great.
            //         In the next .NET Fx, they are expecting to fix XslCompiledTransform so it can pipeline more resonably.  This should be changed at that time.
            foreach (BimlFile bimlFile in xmlIR.BimlFiles)
            {
                XDocument document = bimlFile.XDocument;
                var intermediateXmlDocument = new XmlDocument();
                intermediateXmlDocument.Load(document.CreateReader());
                    if (!String.IsNullOrEmpty(xmlIR.TemplatePath))
                    {
                        foreach (string s in xsltFileList)
                        {
                            var xslt = new XslCompiledTransform();
                            var args = new XsltArgumentList();

                            args.AddParam("TemplatePath", String.Empty, xmlIR.TemplatePath);

                            xslt.Load(s, settings, new XmlUrlResolver());

                            var intermediateMemoryStream = new MemoryStream();
                            xslt.Transform(intermediateXmlDocument, args, intermediateMemoryStream);

                            intermediateMemoryStream.Position = 0;

                            intermediateXmlDocument = new XmlDocument();
                            intermediateXmlDocument.Load(intermediateMemoryStream);
                        }
                    }

                output.AddXml(bimlFile.FilePath, intermediateXmlDocument, bimlFile.EmitType, true);
            }

            output.SchemaSet = xmlIR.SchemaSet;
            output.DefaultXmlNamespace = xmlIR.DefaultXmlNamespace;
            return output;   
        }
        #endregion
    }
}
