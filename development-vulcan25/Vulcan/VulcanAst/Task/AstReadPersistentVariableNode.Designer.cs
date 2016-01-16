//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Varigence Ast Designer tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.  Do not edit this file unless you know exactly
//     you are doing and are synchronized with the Vulcan development team.
//
//     For more information about the Varigence Ast Designer tool, email
//     support@varigence.com.
//
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Collections.Specialized;
using Vulcan.Utility.ComponentModel;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using AstFramework.Markup;
using VulcanEngine.AstFramework;
using Vulcan.Utility.Collections;
using System.Reflection;
using System.Collections.Generic;
using Vulcan.Utility.Xml;
using Vulcan.Utility.Common;
using Vulcan.Utility.Markup;

namespace VulcanEngine.IR.Ast.Task
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    [FriendlyNameAttribute("ReadPersistentVariable")]
    [AstSchemaTypeBindingAttribute("ReadPersistentVariableTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public partial class AstReadPersistentVariableNode : VulcanEngine.IR.Ast.Task.AstTaskNode
    {
        #region Private Storage
        private VulcanEngine.IR.Ast.Connection.AstConnectionNode _connection;
        private bool __isNotFirstSet__connection;

        private VulcanEngine.IR.Ast.PersistentVariables.AstPersistentVariableNode _persistentVariable;
        private bool __isNotFirstSet__persistentVariable;

        private VulcanEngine.IR.Ast.Task.AstVariableNode _targetVariable;
        private bool __isNotFirstSet__targetVariable;

        #endregion Private Storage

        #region Public Accessor Properties
        [VulcanCategory("Required")]
        [AstRequiredProperty]
        [VulcanDescription(@"")]
        [Browsable(true)]
        [AstXNameBinding("ConnectionName", ChildType.Attribute)]
        public VulcanEngine.IR.Ast.Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set
            {
                if (_connection != value || !__isNotFirstSet__connection)
                {
                    __isNotFirstSet__connection = true;
                    if (_connection != null)
                    {
                        _connection.Unreference(this, "Connection");
                    }
                    VulcanEngine.IR.Ast.Connection.AstConnectionNode oldValue = _connection;
                    _connection = value;
                    if (_connection != null)
                    {
                        _connection.Reference(this, "Connection");
                    }
                    VulcanOnPropertyChanged("Connection", oldValue, _connection);
                }
            }
        }


        [VulcanCategory("Required")]
        [AstRequiredProperty]
        [VulcanDescription(@"")]
        [Browsable(true)]
        [AstXNameBinding("PersistentVariableName", ChildType.Attribute)]
        public VulcanEngine.IR.Ast.PersistentVariables.AstPersistentVariableNode PersistentVariable
        {
            get { return _persistentVariable; }
            set
            {
                if (_persistentVariable != value || !__isNotFirstSet__persistentVariable)
                {
                    __isNotFirstSet__persistentVariable = true;
                    if (_persistentVariable != null)
                    {
                        _persistentVariable.Unreference(this, "PersistentVariable");
                    }
                    VulcanEngine.IR.Ast.PersistentVariables.AstPersistentVariableNode oldValue = _persistentVariable;
                    _persistentVariable = value;
                    if (_persistentVariable != null)
                    {
                        _persistentVariable.Reference(this, "PersistentVariable");
                    }
                    VulcanOnPropertyChanged("PersistentVariable", oldValue, _persistentVariable);
                }
            }
        }


        [VulcanCategory("Required")]
        [AstRequiredProperty]
        [VulcanDescription(@"")]
        [Browsable(true)]
        [AstXNameBinding("TargetVariableName", ChildType.Attribute)]
        public VulcanEngine.IR.Ast.Task.AstVariableNode TargetVariable
        {
            get { return _targetVariable; }
            set
            {
                if (_targetVariable != value || !__isNotFirstSet__targetVariable)
                {
                    __isNotFirstSet__targetVariable = true;
                    if (_targetVariable != null)
                    {
                        _targetVariable.Unreference(this, "TargetVariable");
                    }
                    VulcanEngine.IR.Ast.Task.AstVariableNode oldValue = _targetVariable;
                    _targetVariable = value;
                    if (_targetVariable != null)
                    {
                        _targetVariable.Reference(this, "TargetVariable");
                    }
                    VulcanOnPropertyChanged("TargetVariable", oldValue, _targetVariable);
                }
            }
        }


        #endregion Public Accessor Properties

        #region Collection Methods



        #endregion Collection Methods

        #region Initialization
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void InitializeAstNode()
        {
        }

        #endregion Initialization

        public override VulcanCollection<IFrameworkItem> DefinedAstNodes()
        {
            var definedAstNodes = new VulcanCollection<IFrameworkItem>();
            definedAstNodes.AddRange(base.DefinedAstNodes());



            return definedAstNodes;
        }


        public override IEnumerable<IScopeBoundary> BindingScopeBoundaries()
        {
            var bindingScopeBoundaries = new List<IScopeBoundary>();
            bindingScopeBoundaries.Add(ScopeBoundary);
            return bindingScopeBoundaries;
        }


        #region Cloning Support
        public override IFrameworkItem Clone()
        {
            return Clone(ParentItem);
        }

        public override IFrameworkItem Clone(Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping)
        {
            return Clone(ParentItem, cloneMapping);
        }

        public override IFrameworkItem Clone(IFrameworkItem parentItem)
        {
            return Clone(parentItem, new Dictionary<IFrameworkItem, IFrameworkItem>());
        }

        public override IFrameworkItem Clone(IFrameworkItem parentItem, Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping)
        {
            SymbolTable.GetSourceToCloneDefinitionMappings(this, parentItem, cloneMapping);
            CloneInto(cloneMapping[this], cloneMapping);
            return cloneMapping[this];
        }

        public override void CloneInto(IFrameworkItem targetItem, Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping)
        {
            base.CloneInto(targetItem, cloneMapping);
            if (targetItem == null || !typeof(AstReadPersistentVariableNode).IsAssignableFrom(targetItem.GetType()))
            {
                throw new ArgumentException("Provided target node is not of the correct type.");
            }
            var castedTargetItem = (AstReadPersistentVariableNode)targetItem;
            if (_connection == null)
            {
                castedTargetItem._connection = null;
            }
            else if (cloneMapping.ContainsKey(_connection))
            {
                castedTargetItem._connection = (VulcanEngine.IR.Ast.Connection.AstConnectionNode)cloneMapping[_connection];
            }
            else
            {
                castedTargetItem._connection = _connection;
            }

            if (_persistentVariable == null)
            {
                castedTargetItem._persistentVariable = null;
            }
            else if (cloneMapping.ContainsKey(_persistentVariable))
            {
                castedTargetItem._persistentVariable = (VulcanEngine.IR.Ast.PersistentVariables.AstPersistentVariableNode)cloneMapping[_persistentVariable];
            }
            else
            {
                castedTargetItem._persistentVariable = _persistentVariable;
            }

            if (_targetVariable == null)
            {
                castedTargetItem._targetVariable = null;
            }
            else if (cloneMapping.ContainsKey(_targetVariable))
            {
                castedTargetItem._targetVariable = (VulcanEngine.IR.Ast.Task.AstVariableNode)cloneMapping[_targetVariable];
            }
            else
            {
                castedTargetItem._targetVariable = _targetVariable;
            }

        }

        public override IFrameworkItem CloneHusk(IFrameworkItem parentItem)
        {
            return new AstReadPersistentVariableNode(parentItem);
        }

        #endregion Cloning Support


    }
}