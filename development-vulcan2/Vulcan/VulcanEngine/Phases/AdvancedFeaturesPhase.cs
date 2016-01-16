using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

using VulcanEngine.Properties;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;
using VulcanEngine.IR.Ast.Transformation;
using VulcanEngine.IR.Ast.Table;


namespace VulcanEngine.Phases
{
    [PhaseFriendlyName("AdvancedFeaturesPhase")]
    public class AdvancedFeaturesPhase : IPhase
    {
        private string _workflowUniqueName;
        private MessageEngine _message;

        public string Name
        {
            get
            {
                return "AdvancedFeaturesPhase";
            }
        }

        public string WorkflowUniqueName
        {
            get { return this._workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { throw new NotImplementedException(); }
        }

        public AdvancedFeaturesPhase(string WorkflowUniqueName)
        {
            this._workflowUniqueName = WorkflowUniqueName;
            this._message = MessageEngine.Create(WorkflowUniqueName);
        }

        public VulcanEngine.IR.IIR Execute(List<VulcanEngine.IR.IIR> PredecessorIRs)
        {
            return this.Execute(IRUtility.MergeIRList(this.Name, PredecessorIRs));
        }

        public VulcanEngine.IR.IIR Execute(VulcanEngine.IR.IIR PredecessorIR)
        {
            AstIR ir = (AstIR)PredecessorIR;

            /*foreach (AstPackageNode p in ir.AstRootNode.Packages)
            {
                foreach (AstTaskNode t in p)
                {
                    Console.WriteLine("Task! {0}", t.Name);
                }
            } */
            return ProcessTableQuerySources(ir);
            //return ir;
        }

        private AstIR ProcessKeyLookups(AstIR astIR)
        {
            throw new NotImplementedException();
        }

        private AstIR ProcessCustomLookups(AstIR astIR)
        {
            throw new NotImplementedException();
        }

        private AstIR ProcessTableQuerySources(AstIR astIR)
        {
            List<AstTableNode> tables = new List<AstTableNode>();
            tables.AddRange(astIR.AstRootNode.Dimensions.Cast<AstTableNode>());
            tables.AddRange(astIR.AstRootNode.Facts.Cast<AstTableNode>());
            tables.AddRange(astIR.AstRootNode.Tables);

            foreach (AstTableNode table in tables)
            {
                foreach (AstTableQuerySourceNode querySource in table.Sources.OfType<AstTableQuerySourceNode>())
                {
                    AstPackageNode package = new AstPackageNode();
                    package.ConstraintMode = ContainerConstraintMode.Linear;
                    package.DefaultPlatform = PlatformType.SSIS08;
                    package.Log = false;
                    package.Name = querySource.Name;
                    package.Type = "ETL";

                    AstStagingContainerTaskNode staging = new AstStagingContainerTaskNode();
                    staging.ConstraintMode = ContainerConstraintMode.Linear;
                    staging.Log = false;
                    staging.Name = querySource.Name;
                    staging.CreateAs = String.Format("__Staging_{0}_{1}", table.Name, querySource.Name);
                    staging.StagingConnection = table.Connection;
                    staging.Table = table;

                    AstETLRootNode etl = new AstETLRootNode();
                    etl.Name = String.Format("__ETL_Staging_{0}_{1}", table.Name, querySource.Name);
                    etl.DelayValidation = true;
                    
                    AstQuerySourceNode source = new AstQuerySourceNode();
                    source.Connection = querySource.Connection;
                    source.Name = String.Format("__ETL_Staging_Source_{0}_{1}", table.Name, querySource.Name);
                    source.Query = querySource.Query;
                    etl.Transformations.Add(source);

                    AstDestinationNode destination = new AstDestinationNode();
                    destination.AccessMode = DestinationAccessModeFacet.TableFastLoad;
                    destination.CheckConstraints = true;
                    destination.TableLock = true;
                    destination.Connection = table.Connection;
                    destination.Name = String.Format("__ETL_Staging_Destination_{0}_{1}", table.Name, querySource.Name);
                    destination.TableName = staging.CreateAs;
                    destination.ValidateExternalMetadata = false;
                    etl.Transformations.Add(destination);

                    staging.Tasks.Add(etl);

                    AstMergeTaskNode merge = new AstMergeTaskNode();
                    merge.Connection = table.Connection;
                    merge.Name = String.Format("__Staging_Merge_{0}_{1}", table.Name, querySource.Name);
                    merge.SourceName = staging.CreateAs;
                    merge.TargetConstraint = table.PreferredKey;

                    staging.Tasks.Add(merge);

                    package.Tasks.Add(staging);

                    astIR.AstRootNode.Packages.Add(package);
                }
            }
            return astIR;
        }
    }
}
