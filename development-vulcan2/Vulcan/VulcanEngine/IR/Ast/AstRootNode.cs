using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("VulcanRootElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstRootNode : AstNode
    {
        #region Private Storage
        private VulcanCollection<AstIncludedFileNode> _includedFiles;
        private VulcanCollection<Connection.AstConnectionNode> _connections;
        private VulcanCollection<Table.AstTableNode> _tables;
        private VulcanCollection<Dimension.AstDimensionNode> _dimensions;
        private VulcanCollection<DimensionInstance.AstDimensionInstanceNode> _dimensionInstances;
        private VulcanCollection<Fact.AstFactNode> _facts;
        private VulcanCollection<Task.AstPackageNode> _packages;
        private VulcanCollection<Task.AstStoredProcNode> _storedProcs;
        #endregion // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Using", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstIncludedFileNode> IncludedFiles
        {
            get { return _includedFiles; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Connection", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Connection.AstConnectionNode> Connections
        {
            get { return _connections; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Table", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Table.AstTableNode> Tables
        {
            get { return _tables; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Dimension", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Dimension.AstDimensionNode> Dimensions
        {
            get { return _dimensions; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DimensionInstance", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<DimensionInstance.AstDimensionInstanceNode> DimensionInstances
        {
            get { return _dimensionInstances; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Fact", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Fact.AstFactNode> Facts
        {
            get { return _facts; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Package", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Task.AstPackageNode> Packages
        {
            get { return _packages; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("StoredProc", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Task.AstStoredProcNode> StoredProcs
        {
            get { return _storedProcs; }
        }
        #endregion // Public Accessor Properties

        #region Default Constructor
        public AstRootNode()
        {
            this._includedFiles = new VulcanCollection<AstIncludedFileNode>();
            this._connections = new VulcanCollection<Connection.AstConnectionNode>();
            this._tables = new VulcanCollection<VulcanEngine.IR.Ast.Table.AstTableNode>();
            this._dimensions = new VulcanCollection<Dimension.AstDimensionNode>();
            this._dimensionInstances = new VulcanCollection<DimensionInstance.AstDimensionInstanceNode>();
            this._facts = new VulcanCollection<VulcanEngine.IR.Ast.Fact.AstFactNode>();
            this._packages = new VulcanCollection<VulcanEngine.IR.Ast.Task.AstPackageNode>();
            this._storedProcs = new VulcanCollection<VulcanEngine.IR.Ast.Task.AstStoredProcNode>();
        }
        #endregion // Default Constructor

        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Connections.Cast<AstNode>());
                children.AddRange(this.DimensionInstances.Cast<AstNode>());
                children.AddRange(this.Dimensions.Cast<AstNode>());
                children.AddRange(this.Facts.Cast<AstNode>());
                children.AddRange(this.Packages.Cast<AstNode>());
                children.AddRange(this.StoredProcs.Cast<AstNode>());
                children.AddRange(this.Tables.Cast<AstNode>());
                return children;
            }
        }

        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            foreach (AstNode child in this.Children)
            {
                validationItems.AddRange(child.Validate());
            }
            return validationItems;
        }
    }
}

#region Private Storage
#endregion   // Private Storage

#region Public Accessor Properties
#endregion   // Public Accessor Properties

#region Default Constructor
#endregion   // Default Constructor