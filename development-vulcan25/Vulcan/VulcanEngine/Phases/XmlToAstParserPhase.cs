using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using System.Xml;
using AstFramework;
using AstFramework.Engine;
using AstFramework.Engine.Binding;
using Vulcan.Utility.Markup;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.IR.Ast;

namespace VulcanEngine.Phases
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToAst", Justification = "FXCop is incorrectly parsing ToAst as Toast.")]
    [PhaseFriendlyName("XmlToAstParserPhase")]
    public class XmlToAstParserPhase : IPhase
    {
        private readonly string _workflowUniqueName;

        private readonly string _defaultXmlNamespace;

        #region IPhase Members

        public string Name
        {
            get { return "XmlToASTParserPhase"; }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Supplying this attribute would cause a downstream issue with the IPhase interface.")]
        public IIR Execute(IIR predecessorIR)
        {
            var xmlIR = predecessorIR as XmlIR;
            if (xmlIR == null)
            {
                // Message.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, PredecessorIR.GetType().ToString(), this.Name);
            }

            var astIR = new AstIR(xmlIR) { AstRootNode = new AstRootNode(null) };

            var unboundReferences = new UnboundReferences();
            var languageSettings = new LanguageSettings(_defaultXmlNamespace, typeof(AstNode));
            foreach (BimlFile bimlFile in astIR.BimlFiles)
            {
                if (bimlFile.XDocument.Root != null)
                {
                    AstParser.ParseDocument(bimlFile, astIR.AstRootNode, unboundReferences, languageSettings);
                }
            }

            unboundReferences.ResolveAll(astIR.AstRootNode.SymbolTable);
            CompileTimeResolver.ResolveAll(astIR.AstRootNode.SymbolTable, unboundReferences);

            if (unboundReferences.Count > 0)
            {
                foreach (var unboundReference in unboundReferences)
                {
                    string filename = unboundReference.BimlFile.Name;
                    string refName = unboundReference.XValue;
                    string refTypeFriendlyName = unboundReference.BoundProperty.PropertyType.Name;
                    string xml = unboundReference.XObject.ToString();
                    int line = ((IXmlLineInfo)unboundReference.XObject).LineNumber;
                    int offset = ((IXmlLineInfo)unboundReference.XObject).LinePosition;
                    var friendlyNames = (FriendlyNameAttribute[])unboundReference.BoundProperty.PropertyType.GetCustomAttributes(typeof(FriendlyNameAttribute), false);
                    if (friendlyNames != null && friendlyNames.Length > 0)
                    {
                        refTypeFriendlyName = friendlyNames[0].FriendlyName;
                    }

                    // TODO: Fatal Error
                    MessageEngine.Trace(filename, line, offset, Severity.Error, "V0101", null, "Could not resolve reference to '{0}' of type '{1}'. '{2}' is invalid.", refName, refTypeFriendlyName, xml);
                }

                throw new InvalidOperationException("Parsing was unsuccessful.");
            }

            return astIR;
        }

        #endregion

        #region Initialization
        public XmlToAstParserPhase(string workflowUniqueName, string defaultXmlNamespace)
        {
            _workflowUniqueName = workflowUniqueName;
            _defaultXmlNamespace = defaultXmlNamespace;
        }
        #endregion //Initialization
    }
}
