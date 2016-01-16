using System.Collections.Generic;
using AstFramework.Engine.Binding;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableTemplateInstanceNode : ITemplateInstance
    {
        public Dictionary<string, string> ArgumentDictionary
        {
            get
            {
                var argumentDictionary = new Dictionary<string, string>();
                foreach (var mapping in Arguments)
                {
                    argumentDictionary.Add(mapping.ArgumentName, mapping.Value);
                }

                return argumentDictionary;
            }
        }

        public AstTableTemplateInstanceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        public void Instantiate(SymbolTable symbolTable, UnboundReferences unboundReferences)
        {
            var rootNode = ParentItem as AstRootNode;
            var tableTemplate = Template as AstTableTemplateNode;
            var clonedMapping = new Dictionary<IFrameworkItem, IFrameworkItem>();
            if (tableTemplate != null && rootNode != null)
            {
                var clonedTable = (AstTableNode)tableTemplate.Table.Clone(rootNode, clonedMapping);
                clonedTable.Emit = this.Emit;

                // TODO: Some of the ViewModel stuff might not fully support Replace - so simulating with Remove and Insert
                int index = rootNode.Tables.IndexOf(this);
                rootNode.Tables.Remove(this);
                rootNode.Tables.Insert(index, clonedTable);
            }

            foreach (var bindingItem in tableTemplate.UnboundReferences)
            {
                var clonedBindingItem = new BindingItem(
                    bindingItem.BoundProperty,
                    bindingItem.XObject,
                    bindingItem.XValue,
                    clonedMapping[bindingItem.ParentItem],
                    bindingItem.BimlFile,
                    this);
                unboundReferences.Add(clonedBindingItem);
            }
        }

        #region ITemplateInstance Members

        ITemplate ITemplateInstance.Template
        {
            get { return Template; }
        }

        #endregion
    }
}

