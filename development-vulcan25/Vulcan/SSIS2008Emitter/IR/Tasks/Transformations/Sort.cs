using System;
using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using Vulcan.Utility.Collections;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class Sort : SingleOutTransformation
    {
        [Flags]
        private enum SortComparisonOptions : uint
        {
            None = 0x00000000,
            IgnoreCase = 0x00000001,
            IgnoreNonspacingCharacters = 0x00000002,
            IgnoreSymbols = 0x00000004,
            SortPunctuationAsSymbols = 0x00001000,
            IgnoreKanaType = 0x00010000,
            IgnoreCharacterWidth = 0x00020000,
        }

        private readonly AstSortNode _astSortNode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            context.ParentObject.Children.Add(new Sort(context, astNode));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public Sort(LoweringContext context, AstNode astNode)
            : base(context, astNode as AstTransformationNode)
        {
            _astSortNode = astNode as AstSortNode;
            RegisterInputBinding(_astSortNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.Sort"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            Component.CustomPropertyCollection["EliminateDuplicates"].Value = _astSortNode.EliminateDuplicates;
            Component.CustomPropertyCollection["MaximumThreads"].Value = _astSortNode.MaximumThreads;

            ProcessBindings(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            int sortKey = 0;

            if (_astSortNode.AutoPassthrough)
            {
                foreach (string inputColumnName in GetVirtualInputColumns(0))
                {
                    SetInputColumnUsage(0, inputColumnName, DTSUsageType.UT_READONLY, false);
                }
            }

            foreach (AstSortColumnNode sc in _astSortNode.InputColumns)
            {
                if (sc.InputColumnUsageType == SortColumnUsageType.SortColumn || sc.InputColumnUsageType == SortColumnUsageType.Passthrough)
                {
                    IDTSInputColumn100 input = SetInputColumnUsage(0, sc.Name, DTSUsageType.UT_READONLY, false);
                    IDTSOutputColumn100 output = Component.OutputCollection[0].OutputColumnCollection[input.Name];

                    int comparisonFlags = 0;
                    int sortKeyPosition = 0;
                    if (sc.InputColumnUsageType == SortColumnUsageType.SortColumn)
                    {
                        ////uint comparisonFlags = (uint)ProcessSortType(sc.ComparisonFlags);
                        comparisonFlags = (int)ProcessSortType(sc.ComparisonFlags);
                        sortKeyPosition = ++sortKey;

                        output.SortKeyPosition = sortKey;
                        output.ComparisonFlags = (int)comparisonFlags;
                        if (!String.IsNullOrEmpty(sc.OutputAs))
                        {
                            output.Name = sc.OutputAs;
                        }
                    }

                    input.CustomPropertyCollection["NewSortKeyPosition"].Value = sortKeyPosition;
                    input.CustomPropertyCollection["NewComparisonFlags"].Value = comparisonFlags;
                }
                else
                {
                    SetInputColumnUsage(0, sc.Name, DTSUsageType.UT_IGNORED, true);
                }
            }
        }

        private static SortComparisonOptions ProcessSortType(VulcanCollection<SortComparisonFlag> sortFlags)
        {
            SortComparisonOptions sortType = SortComparisonOptions.None;

            foreach (SortComparisonFlag sortFlag in sortFlags)
            {
                switch (sortFlag)
                {
                    case SortComparisonFlag.IgnoreCase:
                        sortType |= SortComparisonOptions.IgnoreCase;
                        break;
                    case SortComparisonFlag.IgnoreCharacterWidth:
                        sortType |= SortComparisonOptions.IgnoreCharacterWidth;
                        break;
                    case SortComparisonFlag.IgnoreKanaType:
                        sortType |= SortComparisonOptions.IgnoreKanaType;
                        break;
                    case SortComparisonFlag.IgnoreNonspacingCharacters:
                        sortType |= SortComparisonOptions.IgnoreNonspacingCharacters;
                        break;
                    case SortComparisonFlag.IgnoreSymbols:
                        sortType |= SortComparisonOptions.IgnoreSymbols;
                        break;
                    case SortComparisonFlag.SortPunctuationAsSymbols:
                        sortType |= SortComparisonOptions.SortPunctuationAsSymbols;
                        break;
                    default:
                        throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ErrorEnumerationTypeNotSupported, sortFlag));
                }
            }

            return sortType;
        }
    }
}