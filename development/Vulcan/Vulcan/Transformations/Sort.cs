/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Vulcan.Common;
using Vulcan.Tasks;
using Vulcan.Common.Templates;
using Vulcan.Packages;
using Vulcan.Emitters;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Vulcan.Transformations
{
    public class Sort : Transformation
    {
        public enum InputColumnUsageType
        {
            PassThrough,
            SortColumn
        }

        public enum SortType
        {
            ASC,
            DESC
        }

        public enum ComparisonFlag
        {
            None,
            IgnoreCase,
            IgnoreKanaType,
            IgnoreNonspacingCharacters,
            IgnoreCharacterWidth,
            IgnoreSymbols,
            SortPunctuationAsSymbols
        }

        IDTSComponentMetaData90 _sortCom;
        CManagedComponentWrapper _sortComI;
        int _sortOrder;

        public Sort(
           VulcanPackage vulcanPackage,
           MainPipe dataFlowTask,
           IDTSComponentMetaData90
           parentComponent,
           string name,
           string description,
           bool eliminateDuplicates,
           int maximumThreads
           )
            :
           base(
           vulcanPackage,
           dataFlowTask,
           parentComponent,
           name,
           description
           )
        {
            _sortOrder = 1;

            _sortCom = dataFlowTask.ComponentMetaDataCollection.New();
            _sortCom.ComponentClassID = "DTSTransform.sort";

            _sortComI = _sortCom.Instantiate();
            _sortComI.ProvideComponentProperties();

            _sortCom.Name = name;

            _sortComI.SetComponentProperty("EliminateDuplicates", eliminateDuplicates);
            _sortComI.SetComponentProperty("MaximumThreads", maximumThreads);
            _sortCom.OutputCollection[0].ErrorRowDisposition = DTSRowDisposition.RD_NotUsed;

            dataFlowTask.PathCollection.New().AttachPathAndPropagateNotifications(
                                                                                  parentComponent.OutputCollection[0],
                                                                                  _sortCom.InputCollection[0]
                                                                                  );
            _sortComI.AcquireConnections(null);
            _sortComI.ReinitializeMetaData();
            _sortComI.ReleaseConnections();
        }

        public void SetInputColumnProperty(string inputColumnName, Sort.InputColumnUsageType inputColumnUsageType, Sort.SortType sortType, List<Sort.ComparisonFlag> comparisonFlagList)
        {
            int sortOrderForCurrentColumn;
            int intComparisonFlag = 0;

            if (inputColumnUsageType == InputColumnUsageType.PassThrough)
            {
                sortOrderForCurrentColumn = 0;
            }
            else
            {
                if (sortType == SortType.ASC)
                {
                    sortOrderForCurrentColumn = _sortOrder;
                    _sortOrder++;
                }
                else if (sortType == SortType.DESC)
                {
                    sortOrderForCurrentColumn = (-1) * _sortOrder;
                    _sortOrder++;
                }
                else
                {
                    sortOrderForCurrentColumn = 0;
                }
            }

            foreach (Sort.ComparisonFlag comparisonFlag in comparisonFlagList)
            {
                switch (comparisonFlag)
                {
                    case ComparisonFlag.None:
                        intComparisonFlag += 0;
                        break;
                    case ComparisonFlag.IgnoreCase:
                        intComparisonFlag += 1;
                        break;
                    case ComparisonFlag.IgnoreKanaType:
                        intComparisonFlag += 65536;
                        break;
                    case ComparisonFlag.IgnoreNonspacingCharacters:
                        intComparisonFlag += 2;
                        break;
                    case ComparisonFlag.IgnoreSymbols:
                        intComparisonFlag += 4;
                        break;
                    case ComparisonFlag.SortPunctuationAsSymbols:
                        intComparisonFlag += 4096;
                        break;
                    default:
                        intComparisonFlag += 0;
                        break;
                }
            }

            SetInputColumnProperty(inputColumnName, sortOrderForCurrentColumn, intComparisonFlag);
        }

        private void SetInputColumnProperty(string inputColumnName, int sortOrder, int intComparisonFlag)
        {
            this.SetInputUsageType(inputColumnName, DTSUsageType.UT_READONLY);
            _sortComI.SetInputColumnProperty(
                _sortCom.InputCollection[0].ID,
                 _sortCom.InputCollection[0].InputColumnCollection[inputColumnName].ID,
                "NewSortKeyPosition",
                sortOrder);

            //Comparison Flags only apply to string columns.
            if (
                _sortCom.InputCollection[0].InputColumnCollection[inputColumnName].DataType == DataType.DT_NTEXT ||
                _sortCom.InputCollection[0].InputColumnCollection[inputColumnName].DataType == DataType.DT_STR ||
                _sortCom.InputCollection[0].InputColumnCollection[inputColumnName].DataType == DataType.DT_TEXT ||
                _sortCom.InputCollection[0].InputColumnCollection[inputColumnName].DataType == DataType.DT_WSTR
                )
            {
                _sortComI.SetInputColumnProperty(
                    _sortCom.InputCollection[0].ID,
                     _sortCom.InputCollection[0].InputColumnCollection[inputColumnName].ID,
                    "NewComparisonFlags",
                    intComparisonFlag);
            }
        }


        public override IDTSComponentMetaData90 Component
        {
            get
            {
                return this._sortCom;
            }
        }

        public override CManagedComponentWrapper ComponentInstance
        {
            get
            {
                return this._sortComI;
            }
        }
    }
}