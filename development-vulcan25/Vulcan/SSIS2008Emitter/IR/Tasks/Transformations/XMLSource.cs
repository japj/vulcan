using System;
using System.Globalization;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class XmlSource : Transformation
    {
        private readonly AstXmlSourceNode _astXmlSourceNode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            context.ParentObject.Children.Add(new XmlSource(context, astNode));
        }

        public XmlSource(LoweringContext context, AstNode astNode) : base(context, astNode as AstTransformationNode)
        {
            _astXmlSourceNode = astNode as AstXmlSourceNode;
        }

        public override string Moniker
        {
            get { return "Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            int accessMode;
            string xmlData = string.Empty;
            string xmlDataVariable = string.Empty;

            switch (_astXmlSourceNode.XmlDataAccessMode)
            {
                case XmlSourceDataAccessMode.XmlFileLocation: 
                    accessMode = 0;
                    xmlData = _astXmlSourceNode.XmlData; 
                    break;
                case XmlSourceDataAccessMode.XmlFileFromVariable: 
                    accessMode = 1;
                    xmlDataVariable = _astXmlSourceNode.XmlData;
                    break;
                case XmlSourceDataAccessMode.XmlDataFromVariable: 
                    accessMode = 2;
                    xmlDataVariable = _astXmlSourceNode.XmlData;
                    break;
                default: 
                    throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture, "XmlSource: Data Access Mode Type {0} Not Implemented", _astXmlSourceNode.XmlDataAccessMode));
            }

            Instance.SetComponentProperty("XMLData", xmlData);
            Instance.SetComponentProperty("XMLDataVariable", xmlDataVariable);
            Instance.SetComponentProperty("XMLSchemaDefinition", _astXmlSourceNode.XmlSchemaDefinition);
            Instance.SetComponentProperty("UseInlineSchema", _astXmlSourceNode.UseInlineSchema);
            Instance.SetComponentProperty("AccessMode", accessMode);

            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
        }
    }
}

