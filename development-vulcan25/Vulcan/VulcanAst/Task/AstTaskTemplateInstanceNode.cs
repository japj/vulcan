using System.Collections.Generic;
using System.Collections.Specialized;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstTaskTemplateInstanceNode : ITemplateInstance
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

        public AstTaskTemplateInstanceNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }

        public void Instantiate(SymbolTable symbolTable, UnboundReferences unboundReferences)
        {
            var parentContainer = ParentItem as AstContainerTaskBaseNode;
            var taskTemplate = Template as AstTaskTemplateNode;
            var clonedMapping = new Dictionary<IFrameworkItem, IFrameworkItem>();
            if (taskTemplate != null && parentContainer != null)
            {
                var clonedTasks = new List<AstTaskNode>();
                foreach (var task in taskTemplate.Tasks)
                {
                    clonedTasks.Add((AstTaskNode)task.Clone(parentContainer, clonedMapping));
                }

                parentContainer.Tasks.Replace(this, clonedTasks);
            }

            foreach (var bindingItem in taskTemplate.UnboundReferences)
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

