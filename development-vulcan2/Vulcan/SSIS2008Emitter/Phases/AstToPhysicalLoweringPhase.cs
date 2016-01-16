using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.IR.Ast;
using Connection = VulcanEngine.IR.Ast.Connection;
using Cube = VulcanEngine.IR.Ast.Cube;
using Dimension = VulcanEngine.IR.Ast.Dimension;
using DimensionInstance = VulcanEngine.IR.Ast.DimensionInstance;
using Fact = VulcanEngine.IR.Ast.Fact;
using MeasureGroup = VulcanEngine.IR.Ast.MeasureGroup;
using Table = VulcanEngine.IR.Ast.Table;
using Transformation = VulcanEngine.IR.Ast.Transformation;
using Task = VulcanEngine.IR.Ast.Task;

using VulcanEngine.Common;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;

namespace VulcanEngine.Phases
{
    [PhaseFriendlyName("AstToPhysicalLoweringPhase")]
    public class AstToPhysicalLoweringPhase : IPhase
    {
        protected string _workflowUniqueName;

        #region IPhase Members

        public string Name
        {
            get { return "XmlToAstParserPhase"; }
        }

        public string WorkflowUniqueName
        {
            get { return this._workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(AstIR); }
        }

        public VulcanEngine.IR.IIR Execute(List<VulcanEngine.IR.IIR> predecessorIRList)
        {
            return this.Execute(IRUtility.MergeIRList(this.Name, predecessorIRList));
        }

        public VulcanEngine.IR.IIR Execute(VulcanEngine.IR.IIR predecessorIR)
        {
            AstIR astIR = predecessorIR as AstIR;
            if (astIR == null)
            {
                // TODO: Message.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, PredecessorIR.GetType().ToString(), this.Name);
            }

            PhysicalIR physicalIR = new PhysicalIR(astIR);

            try
            {
                foreach (Task.AstPackageNode packageNode in astIR.AstRootNode.Packages)
                {
                    if (packageNode.Emit && !packageNode.AsClassOnly)
                    {
                        Package package = packageNode.Lower();
                        if (String.IsNullOrEmpty(package.Type))
                        {
                            package.Type = "ETL";
                        }
                        physicalIR.PhysicalNodes.Add(package);
                    }
                }

                foreach (Dimension.AstDimensionNode dimensionNode in astIR.AstRootNode.Dimensions)
                {
                    if (dimensionNode.Emit && !dimensionNode.AsClassOnly)
                    {
                        Package package = dimensionNode.Lower().Lower();
                        package.Type = "Dimension";
                        physicalIR.PhysicalNodes.Add(package);
                    }
                }

                foreach (Fact.AstFactNode factNode in astIR.AstRootNode.Facts)
                {
                    if (factNode.Emit && !factNode.AsClassOnly)
                    {
                        Package package = factNode.Lower().Lower();
                        package.Type = "FactTable";
                        physicalIR.PhysicalNodes.Add(package);
                    }
                }

                foreach (Connection.AstConnectionNode connectionNode in astIR.AstRootNode.Connections)
                {
                    physicalIR.PhysicalNodes.Add(connectionNode.Lower());
                }

                foreach (Table.AstTableNode tableNode in astIR.AstRootNode.Tables)
                {
                    if (tableNode.Emit && !tableNode.AsClassOnly)
                    {
                        Package package = tableNode.Lower().Lower();
                        package.Type = "Table";
                        physicalIR.PhysicalNodes.Add(package);

                        foreach (Table.AstTableSourceBaseNode sourceNode in tableNode.Sources)
                        {
                            Table.AstTableDynamicSourceNode dynamicSource = sourceNode as Table.AstTableDynamicSourceNode;
                            if (dynamicSource != null)
                            {
                                Package sourcePackage = dynamicSource.Package.Lower();
                                sourcePackage.Type = "ETL";
                                physicalIR.PhysicalNodes.Add(sourcePackage);
                            }
                        }
                    }
                }
            }
            catch (Ssis2008Emitter.SSISEmitterException EmitterException)
            {
                StringBuilder err = new StringBuilder();
                err.Append("\r\nCompiling ");

                Exception e = EmitterException;
                bool bIsNullException = false;
                while (e != null)
                {
                    err.AppendFormat("{0}\r\n", e.Message);
                    if (e.InnerException != null)
                    {
                        e = (e.InnerException);
                        err.AppendFormat("-->\t");
                    }
                    else
                    {
                        if (e is NullReferenceException)
                        {
                            bIsNullException = true;
                        }
                        e = null;
                    }
                }
                MessageEngine.Global.Trace(Severity.Error, err.ToString());
                if (bIsNullException)
                {
                    MessageEngine.Global.Trace(Severity.Warning,
                    "Possible reason: an attribute or element is referencing undefined code." + 
                    "To solve this issue, fix the mismatched reference or use <Using> to include the definition.");
                }
            }

            return physicalIR;
        }
        #endregion

        #region Initialization
        public AstToPhysicalLoweringPhase(string WorkflowUniqueName)
        {
            this._workflowUniqueName = WorkflowUniqueName;
        }
        #endregion  // Initialization

    }
}
