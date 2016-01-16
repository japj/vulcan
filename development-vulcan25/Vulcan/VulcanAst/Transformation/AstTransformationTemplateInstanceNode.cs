using System.Collections.Generic;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using VulcanEngine.IR.Ast.Task;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstTransformationTemplateInstanceNode : ITemplateInstance
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

        public AstTransformationTemplateInstanceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        public void Instantiate(SymbolTable symbolTable, UnboundReferences unboundReferences)
        {
            var parentEtl = ParentItem as AstEtlRootNode;
            var transformationTemplate = Template as AstTransformationTemplateNode;
            var clonedMapping = new Dictionary<IFrameworkItem, IFrameworkItem>();
            if (transformationTemplate != null && parentEtl != null)
            {
                var clonedTransformations = new List<AstTransformationNode>();
                foreach (var transformation in transformationTemplate.Transformations)
                {
                    clonedTransformations.Add((AstTransformationNode)transformation.Clone(parentEtl, clonedMapping));
                }

                parentEtl.Transformations.Replace(this, clonedTransformations);
            }

            foreach (var bindingItem in transformationTemplate.UnboundReferences)
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

