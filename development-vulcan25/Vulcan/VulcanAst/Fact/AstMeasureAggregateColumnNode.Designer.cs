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

namespace VulcanEngine.IR.Ast.Fact
{
	[FriendlyNameAttribute("Measure Aggregate Column")]
	[AstSchemaTypeBindingAttribute("AggregateElemType", "http://tempuri.org/vulcan2.xsd")]
	public partial class AstMeasureAggregateColumnNode : VulcanEngine.IR.Ast.Table.AstTableColumnReferenceNode
	{
		#region Private Storage
		private VulcanEngine.IR.Ast.Ssas.SsasAggregationFunction _function;
		private bool __isNotFirstSet__function;

		#endregion Private Storage

		#region Public Accessor Properties
		[VulcanCategory("Required")]
		[AstRequiredProperty]
		[VulcanDescription(@"")]
		[Browsable(true)]
		[AstXNameBinding("Function", ChildType.Attribute)]
		public VulcanEngine.IR.Ast.Ssas.SsasAggregationFunction Function
		{
			get { return _function; }
			set
			{
				if (_function != value || !__isNotFirstSet__function)
				{
					__isNotFirstSet__function = true;
					VulcanEngine.IR.Ast.Ssas.SsasAggregationFunction oldValue = _function;
					_function = value;
					VulcanOnPropertyChanged("Function", oldValue, _function);
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
			if (targetItem == null || !typeof(AstMeasureAggregateColumnNode).IsAssignableFrom(targetItem.GetType()))
			{
				throw new ArgumentException("Provided target node is not of the correct type.");
			}
			var castedTargetItem = (AstMeasureAggregateColumnNode)targetItem;
			castedTargetItem._function = _function;

		}

		public override IFrameworkItem CloneHusk(IFrameworkItem parentItem)
		{
			return new AstMeasureAggregateColumnNode(parentItem);
		}

		#endregion Cloning Support


	}
}
