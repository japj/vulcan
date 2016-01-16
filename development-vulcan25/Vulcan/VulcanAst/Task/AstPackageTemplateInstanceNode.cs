using System.Collections.Generic;
using AstFramework.Engine.Binding;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstPackageTemplateInstanceNode : ITemplateInstance, IEmittableAstNode
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

        public AstPackageTemplateInstanceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        public void Instantiate(SymbolTable symbolTable, UnboundReferences unboundReferences)
        {
            var rootNode = ParentItem as AstRootNode;
            var packageTemplate = Template as AstPackageTemplateNode;
            var clonedMapping = new Dictionary<IFrameworkItem, IFrameworkItem>();
            if (packageTemplate != null && rootNode != null)
            {
                var clonedPackage = (AstPackageNode)packageTemplate.Package.Clone(rootNode, clonedMapping);
                clonedPackage.Emit = this.Emit;

                // TODO: Some of the ViewModel stuff might not fully support Replace - so simulating with Remove and Insert
                int index = rootNode.Packages.IndexOf(this);
                rootNode.Packages.Remove(this);
                rootNode.Packages.Insert(index, clonedPackage);
            }

            foreach (var bindingItem in packageTemplate.UnboundReferences)
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

        #region IEmittableAstNode Members

        public bool Emit
        {
            get;
            set;
        }

        #endregion
    }
}

