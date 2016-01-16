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
using Vulcan.Emitters;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Vulcan.Transformations
{
    public static class TransformationFactory
    {
        public static Transformation ProcessTransformation(Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator transformationsNav)
        {
            Transformation t = null;
            if (transformationsNav != null)
            {
                try
                {
                    switch (transformationsNav.Name)
                    {
                        case "OLEDBCommand":
                            t = CreateOLEDBCommandFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        case "Lookup":
                            t = CreateLookupFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        case "DerivedColumns":
                            t = CreateDerivedColumnsFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        case "IsNullPatcher":
                            t = CreateIsNullPatcherFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        case "TermLookup":
                            t = CreateTermLookupFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        case "ConditionalSplit":
                            t = CreateConditionalSplitFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        case "UnionAll":
                            t = CreateUnionAllFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        case "Sort":
                            t = CreateSortFromXml(vulcanPackage, parentComponent, dataFlowTask, transformationsNav);
                            break;
                        default:
                            break;
                    }
                }
                catch (System.Runtime.InteropServices.COMException ce)
                {
                    Message.Trace(Severity.Error, ce, "COMException in transformation {0}\n {1}\n", transformationsNav.Name, ce.Message);
                }
                catch (Exception e)
                {
                    Message.Trace(Severity.Error, e, "Exception in transformation {0}\n {1}\n", transformationsNav.Name, e.Message);
                }
            }
            return t;
        }

        private static Transformation CreateSortFromXml(Vulcan.Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator sortNav)
        {
            if (sortNav == null || sortNav.Name.ToUpperInvariant() != "Sort".ToUpperInvariant())
            {
                return null;
            }
            string sortName = sortNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
            bool eliminateDuplicates = sortNav.SelectSingleNode("@EliminateDuplicates", vulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean;
            int maximumThreads = sortNav.SelectSingleNode("@MaximumThreads", vulcanPackage.VulcanConfig.NamespaceManager).ValueAsInt;

            //MaximumThreads can not be 0, according to SSIS
            if (maximumThreads == 0)
            {
                maximumThreads = -1;
            }

            Message.Trace(Severity.Debug, "Begin: UnionAll Transformation {0}", sortName);
            Sort sortTask = new Sort(
                  vulcanPackage,
                dataFlowTask,
                parentComponent,
                sortName,
                sortName,
                eliminateDuplicates,
                maximumThreads
                );

            foreach (XPathNavigator navInput in sortNav.Select("rc:InputColumn", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                string inputColumnName = navInput.SelectSingleNode("@InputColumnName", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                Sort.InputColumnUsageType inputColumnUsageType = (Sort.InputColumnUsageType)Enum.Parse(typeof(Sort.InputColumnUsageType), navInput.SelectSingleNode("@InputColumnUsageType").Value);
                Sort.SortType SortType = (Sort.SortType)Enum.Parse(typeof(Sort.SortType), navInput.SelectSingleNode("@SortType").Value);

                List<Sort.ComparisonFlag> comparisonFlagList = new List<Sort.ComparisonFlag>();
                foreach (XPathNavigator navComparisonFlag in navInput.Select("rc:ComparisonFlag", vulcanPackage.VulcanConfig.NamespaceManager))
                {
                    comparisonFlagList.Add((Sort.ComparisonFlag)Enum.Parse(typeof(Sort.ComparisonFlag), navComparisonFlag.Value));
                }
                sortTask.SetInputColumnProperty(inputColumnName, inputColumnUsageType, SortType, comparisonFlagList);
            }

            return sortTask;
        }

        private static Transformation CreateUnionAllFromXml(Vulcan.Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator unionAllNav)
        {
            if (unionAllNav == null || unionAllNav.Name.ToUpperInvariant() != "UnionAll".ToUpperInvariant())
            {
                return null;
            }
            string unionAllName = unionAllNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
            Message.Trace(Severity.Debug, "Begin: UnionAll Transformation {0}", unionAllName);
            UnionAll ua = new UnionAll(
                  vulcanPackage,
                dataFlowTask,
                parentComponent,
                unionAllName,
                unionAllName
                );


            foreach (XPathNavigator navInput in unionAllNav.Select("rc:SourceComponent", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                string sourceComponentName = navInput.SelectSingleNode("@SourceComponentName", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                ua.Component.InputCollection.New().Name = "Input for " + sourceComponentName;

                ua.MapInput(sourceComponentName, dataFlowTask);

                foreach (XPathNavigator navInputColumn in navInput.Select("rc:Map", vulcanPackage.VulcanConfig.NamespaceManager))
                {
                    string sourceColumnName = navInputColumn.SelectSingleNode("@Source", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                    string destinationColumnName;
                    bool unMap = false;
                    if (navInputColumn.SelectSingleNode("@Destination") == null)
                    {
                        unMap = true;
                        destinationColumnName = sourceColumnName;
                    }
                    else
                    {
                        destinationColumnName = navInputColumn.SelectSingleNode("@Destination", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                    }
                    ua.MapInputColumn(sourceComponentName, sourceColumnName, destinationColumnName, unMap);
                }

            }

            ua.InitializeAndMapDestination();
            return ua;

        }

        private static Transformation CreateConditionalSplitFromXml(Vulcan.Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator conditionalSplitNav)
        {
            if (conditionalSplitNav == null || conditionalSplitNav.Name.ToUpperInvariant() != "ConditionalSplit".ToUpperInvariant())
            {
                return null;
            }
            string conditionalSplitName = conditionalSplitNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
            Message.Trace(Severity.Debug, "Begin: ConditionalSplit Transformation {0}", conditionalSplitName);
            ConditionalSplit cs = new ConditionalSplit(
                vulcanPackage,
                dataFlowTask,
                parentComponent,
                conditionalSplitName,
                conditionalSplitName
                );

            int intEvaluationOrder = 0;
            string expression = string.Empty;
            foreach (XPathNavigator nav in conditionalSplitNav.Select("//rc:Output|//rc:DefaultOutput", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                if (nav.Name == "DefaultOutput")
                {
                    cs.Component.OutputCollection.SetIndex(cs.Component.OutputCollection.GetObjectIndexByID(cs.Component.OutputCollection["Conditional Split Default Output"].ID), 0);
                }
                else
                {
                    expression = nav.SelectSingleNode("rc:Expression", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                    IDTSOutput90 newPath = cs.Component.OutputCollection.New();
                    newPath.Name = nav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                    newPath.Description = nav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                    newPath.ExclusionGroup = cs.Component.OutputCollection["Conditional Split Default Output"].ExclusionGroup;
                    newPath.SynchronousInputID = cs.Component.OutputCollection["Conditional Split Default Output"].SynchronousInputID;
                    newPath.ErrorOrTruncationOperation = "Computation";
                    newPath.ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
                    newPath.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;

                    IDTSCustomProperty90 propEvaluationOrder = newPath.CustomPropertyCollection.New();
                    propEvaluationOrder.Name = "EvaluationOrder";
                    propEvaluationOrder.Value = intEvaluationOrder;

                    IDTSCustomProperty90 propFriendlyExpression = newPath.CustomPropertyCollection.New();
                    propFriendlyExpression.Name = "FriendlyExpression";
                    propFriendlyExpression.Value = expression;

                    IDTSCustomProperty90 propExpression = newPath.CustomPropertyCollection.New();
                    propExpression.Name = "Expression";
                    propExpression.Value = expression;

                    //A workaround to connect the path to Conditional Spit's output, 
                    //because we always connect the current task to the previous task's first output
                    cs.Component.OutputCollection.SetIndex(cs.Component.OutputCollection.GetObjectIndexByID(newPath.ID), 0);
                    intEvaluationOrder++;
                }

                IDTSComponentMetaData90 startComponent = cs.Component;

                XPathNavigator transNav = nav.SelectSingleNode("rc:ConditionalSplitOutputPath/rc:Transformations", vulcanPackage.VulcanConfig.NamespaceManager);
                if (transNav != null)
                {
                    foreach (XPathNavigator etlNav in transNav.SelectChildren(XPathNodeType.Element))
                    {
                        // this is naughty but can be fixed later :)
                        Transformation t = TransformationFactory.ProcessTransformation(vulcanPackage, startComponent, dataFlowTask, etlNav);
                        if (t != null)
                        {
                            startComponent = t.Component;
                        }
                    }
                }

                XPathNavigator destNav = nav.SelectSingleNode("rc:ConditionalSplitOutputPath/rc:Destination", vulcanPackage.VulcanConfig.NamespaceManager);
                if (destNav != null)
                {
                    string name = destNav.SelectSingleNode("@Name").Value;
                    Connection destConnection = Connection.GetExistingConnection(vulcanPackage, destNav.SelectSingleNode("@ConnectionName").Value);
                    string tableName = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", destNav.SelectSingleNode("@Table").Value.Trim());
                    OLEDBDestination oledbDestination = new OLEDBDestination(vulcanPackage, dataFlowTask, startComponent, name, name, destConnection, tableName);

                    string accessMode = destNav.SelectSingleNode("@AccessMode").Value;
                    bool tableLock = destNav.SelectSingleNode("@TableLock").ValueAsBoolean;
                    bool checkConstraints = destNav.SelectSingleNode("@CheckConstraints").ValueAsBoolean;
                    bool keepIdentity = destNav.SelectSingleNode("@KeepIdentity").ValueAsBoolean;
                    bool keepNulls = destNav.SelectSingleNode("@KeepNulls").ValueAsBoolean;

                    int rowsPerBatch = destNav.SelectSingleNode("@RowsPerBatch").ValueAsInt;
                    int maxInsertCommitSize = destNav.SelectSingleNode("@MaximumInsertCommitSize").ValueAsInt;

                    switch (accessMode.ToUpperInvariant())
                    {
                        case "TABLE":
                            oledbDestination.ComponentInstance.SetComponentProperty("AccessMode", 0);
                            oledbDestination.ComponentInstance.SetComponentProperty("OpenRowset", tableName);
                            break;
                        case "TABLEFASTLOAD":
                            oledbDestination.ComponentInstance.SetComponentProperty("AccessMode", 3);
                            oledbDestination.ComponentInstance.SetComponentProperty("OpenRowset", tableName);

                            oledbDestination.ComponentInstance.SetComponentProperty("FastLoadKeepIdentity", keepIdentity);
                            oledbDestination.ComponentInstance.SetComponentProperty("FastLoadKeepNulls", keepNulls);
                            oledbDestination.ComponentInstance.SetComponentProperty("FastLoadMaxInsertCommitSize", maxInsertCommitSize);

                            StringBuilder fastLoadOptions = new StringBuilder();
                            if (tableLock)
                            {
                                fastLoadOptions.AppendFormat("TABLOCK,");
                            }
                            if (checkConstraints)
                            {
                                fastLoadOptions.AppendFormat("CHECK_CONSTRAINTS,");
                            }
                            if (rowsPerBatch > 0)
                            {
                                fastLoadOptions.AppendFormat("ROWS_PER_BATCH = {0}", rowsPerBatch);
                            }
                            fastLoadOptions = fastLoadOptions.Replace(",", "", fastLoadOptions.Length - 5, 5);

                            oledbDestination.ComponentInstance.SetComponentProperty("FastLoadOptions", fastLoadOptions.ToString());
                            break;
                        default:
                            Message.Trace(Severity.Error, "Unknown Destination Load Type of {0}", accessMode);
                            break;
                    }

                    try
                    {
                        oledbDestination.InitializeAndMapDestination();
                    }
                    catch (Exception)
                    {
                    }

                    // Map any overrides
                    foreach (XPathNavigator navMap in destNav.Select("rc:Map", vulcanPackage.VulcanConfig.NamespaceManager))
                    {
                        string source = navMap.SelectSingleNode("@Source").Value;
                        string destination;
                        bool unMap = false;

                        if (navMap.SelectSingleNode("@Destination") == null)
                        {
                            unMap = true;
                            destination = source;
                        }
                        else
                        {
                            destination = nav.SelectSingleNode("@Destination").Value;
                        }

                        oledbDestination.Map(source, destination, unMap);
                    }
                } // end DestNav != null
                //this.FirstExecutableGeneratedByPattern = pipeHost;
                //this.LastExecutableGeneratedByPattern = pipeHost;




            }

            return cs;

        }

        private static Transformation CreateTermLookupFromXml(Vulcan.Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator termLookupNav)
        {
            if (termLookupNav == null || termLookupNav.Name.ToUpperInvariant() != "TermLookup".ToUpperInvariant())
            {
                return null;
            }
            string termLookupName = termLookupNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
            bool isCaseSensitive = termLookupNav.SelectSingleNode("@IsCaseSensitive").ValueAsBoolean;
            string refTermColumn = termLookupNav.SelectSingleNode("@RefTermColumn").Value;
            string refTermTable = termLookupNav.SelectSingleNode("@RefTermTable").Value;

            Message.Trace(Severity.Debug, "Begin: TermLookup Transformation {0}", termLookupName);
            Connection sourceConnection =
               Connection.GetExistingConnection(vulcanPackage, termLookupNav.SelectSingleNode("@ConnectionName").Value);
            TermLookup tl = new TermLookup(
                vulcanPackage,
                dataFlowTask,
                parentComponent,
                termLookupName,
                termLookupName,
                sourceConnection,
                isCaseSensitive,
                refTermColumn,
                refTermTable
                );

            foreach (XPathNavigator nav in termLookupNav.Select("rc:InputColumn", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                string inputColumnName = nav.SelectSingleNode("@InputColumnName").Value;
                TermLookup.InputColumnUsageType inputColumnUsageType = (TermLookup.InputColumnUsageType)Enum.Parse(typeof(TermLookup.InputColumnUsageType), nav.SelectSingleNode("@InputColumnUsageType").Value);
                tl.MapInput(inputColumnName, inputColumnUsageType);
            }
            return tl;
        }

        public static DerivedColumns CreateDerivedColumnsFromXml(Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator derivedNav)
        {
            if (derivedNav == null || derivedNav.Name.ToUpperInvariant() != "DerivedColumns".ToUpperInvariant())
            {
                //We don't handle this.
                return null;
            }

            string componentName = derivedNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
            Message.Trace(Severity.Debug, "Begin: DerivedColumns Transformation {0}", componentName);

            DerivedColumns dc = new DerivedColumns(vulcanPackage, dataFlowTask, parentComponent, componentName, componentName);

            foreach (XPathNavigator nav in derivedNav.Select("rc:Column", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                string colName = nav.SelectSingleNode("@Name").Value;
                string typeAsString = nav.SelectSingleNode("@Type").Value;
                int length = nav.SelectSingleNode("@Length").ValueAsInt;
                int precision = nav.SelectSingleNode("@Precision").ValueAsInt;
                int scale = nav.SelectSingleNode("@Scale").ValueAsInt;
                int codepage = nav.SelectSingleNode("@Codepage").ValueAsInt;

                string expression = nav.Value;

                DataType type = TransformationFactory.GetDataTypeFromString(typeAsString);

                dc.AddOutputColumn(colName, type, expression, length, precision, scale, codepage);
            }
            return dc;
        }
        public static Lookup CreateLookupFromXml(Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator lookupNav)
        {
            if (lookupNav == null || lookupNav.Name.ToUpperInvariant() != "Lookup".ToUpperInvariant())
            {
                return null;
            }

            string lookupName = lookupNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
            Message.Trace(Severity.Debug, "Begin: Lookup Transformation {0}", lookupName);
            Connection sourceConnection =
                Connection.GetExistingConnection(
                                                vulcanPackage,
                                                lookupNav
                                                );

            string query = lookupNav.SelectSingleNode("rc:Query", vulcanPackage.VulcanConfig.NamespaceManager) == null
                ? null
                : lookupNav.SelectSingleNode("rc:Query", vulcanPackage.VulcanConfig.NamespaceManager).Value;

            string table = lookupNav.SelectSingleNode("rc:Table", vulcanPackage.VulcanConfig.NamespaceManager) == null
                ? null
                : lookupNav.SelectSingleNode("rc:Table", vulcanPackage.VulcanConfig.NamespaceManager).Value;


            if (table != null)
            {
                SelectEmitter se = new SelectEmitter(vulcanPackage, table, "*");
                se.Emit(out query);
            }

            Lookup l = new Lookup(vulcanPackage, dataFlowTask, parentComponent, lookupName, lookupName, sourceConnection, query);

            foreach (XPathNavigator inputNav in lookupNav.Select("rc:Input", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                string inputName = inputNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                string joinToName = inputNav.SelectSingleNode("rc:JoinToReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager) == null
                                       ? null
                                       : inputNav.SelectSingleNode("rc:JoinToReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager).Value;

                string copyFromName = inputNav.SelectSingleNode("rc:CopyFromReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager) == null
                                       ? null
                                       : inputNav.SelectSingleNode("rc:CopyFromReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager).Value;

                if (joinToName != null)
                {
                    l.MapInput(inputName, joinToName, Lookup.MapType.JoinToReferenceColumn);
                }
                else if (copyFromName != null)
                {
                    l.MapInput(inputName, copyFromName, Lookup.MapType.CopyFromReferenceColumn);
                }
                else
                {
                    Message.Trace(Severity.Error, "Adding Lookup Type: Must supply either a JoinToName or CopyFromName when mapping inputs.");
                }
            }

            foreach (XPathNavigator outputNav in lookupNav.Select("rc:Output", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                string outputName = outputNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
                string joinToName = outputNav.SelectSingleNode("rc:JoinToReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager) == null
                                       ? null
                                       : outputNav.SelectSingleNode("rc:JoinToReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager).Value;

                string copyFromName = outputNav.SelectSingleNode("rc:CopyFromReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager) == null
                                       ? null
                                       : outputNav.SelectSingleNode("rc:CopyFromReferenceColumn", vulcanPackage.VulcanConfig.NamespaceManager).Value;

                if (joinToName != null)
                {
                    l.MapOutput(outputName, joinToName, Lookup.MapType.JoinToReferenceColumn);
                }
                else if (copyFromName != null)
                {
                    l.MapOutput(outputName, copyFromName, Lookup.MapType.CopyFromReferenceColumn);
                }
                else
                {
                    throw new Exception("Vulcan: ETLPattern: Adding Lookup Type: Must supply either a JoinToName or CopyFromName when mapping outputs.");
                }
            }
            return l;
        }
        public static OLEDBCommand CreateOLEDBCommandFromXml(Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator commandNav)
        {
            if (commandNav == null || commandNav.Name.ToUpperInvariant() != "OLEDBCommand".ToUpperInvariant())
            {
                return null;
            }

            string commandName = commandNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;

            Message.Trace(Severity.Debug, "Begin: OLEDB Transformation {0}", commandName);
            Connection destConnection =
                Connection.GetExistingConnection(
                                                vulcanPackage,
                                                commandNav.SelectSingleNode("rc:DestinationConnection/@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value
                                                );

            string command = commandNav.SelectSingleNode("rc:Command", vulcanPackage.VulcanConfig.NamespaceManager).Value;

            OLEDBCommand dbc = new OLEDBCommand(vulcanPackage, dataFlowTask, parentComponent, commandName, commandName, destConnection, command);

            foreach (XPathNavigator nav in commandNav.Select("rc:Map", vulcanPackage.VulcanConfig.NamespaceManager))
            {

                string source = nav.SelectSingleNode("@Source").Value;
                string destination = nav.SelectSingleNode("@Destination") == null
                                     ? "@" + source
                                     : nav.SelectSingleNode("@Destination").Value;
                dbc.Map(source, destination, false);
            }
            return dbc;
        }
        public static DerivedColumns CreateIsNullPatcherFromXml(Packages.VulcanPackage vulcanPackage, IDTSComponentMetaData90 parentComponent, MainPipe dataFlowTask, XPathNavigator nullNav)
        {
            if (nullNav == null || nullNav.Name.ToUpperInvariant() != "IsNullPatcher".ToUpperInvariant())
            {
                return null;
            }

            string componentName = nullNav.SelectSingleNode("@Name", vulcanPackage.VulcanConfig.NamespaceManager).Value;
            Message.Trace(Severity.Debug, "Begin: IsNullPatcher variant DerivedColumns Transformation {0}", componentName);
            DerivedColumns dc = new DerivedColumns(vulcanPackage, dataFlowTask, parentComponent, componentName, componentName);

            IDTSVirtualInput90 vi = dc.Component.InputCollection[0].GetVirtualInput();

            TemplateEmitter te = new TemplateEmitter("NullPatcherIsnullTemplate", vulcanPackage, null);

            foreach (XPathNavigator nav in nullNav.Select("rc:Column", vulcanPackage.VulcanConfig.NamespaceManager))
            {
                string name = nav.SelectSingleNode("@Name").Value;
                string defaultValue = nav.SelectSingleNode("@DefaultValue").Value;

                dc.SetInputUsageType(vi, vi.VirtualInputColumnCollection[name], DTSUsageType.UT_READWRITE);
                IDTSInputColumn90 inputCol = dc.Component.InputCollection[0].InputColumnCollection[name];

                string expression;
                te.SetParameters("#" + vi.VirtualInputColumnCollection[name].LineageID.ToString(), defaultValue);
                te.Emit(out expression);

                string friendlyExpression;
                te.SetParameters(name, defaultValue);
                te.Emit(out friendlyExpression);

                inputCol.CustomPropertyCollection["Expression"].Value = expression;
                inputCol.CustomPropertyCollection["FriendlyExpression"].Value = friendlyExpression;
            }
            return dc;
        }

        private static DataType GetDataTypeFromString(string typeAsString)
        {
            DataType type = DataType.DT_STR;

            switch (typeAsString.ToUpperInvariant())
            {
                case "BOOL":
                    type = DataType.DT_BOOL;
                    break;
                case "FLOAT":
                    type = DataType.DT_R4;
                    break;
                case "DOUBLE":
                    type = DataType.DT_R8;
                    break;
                case "WSTR":
                    type = DataType.DT_WSTR;
                    break;
                case "INT32":
                    type = DataType.DT_I4;
                    break;
                case "INT64":
                    type = DataType.DT_I8;
                    break;
                case "UINT32":
                    type = DataType.DT_UI4;
                    break;
                case "UINT64":
                    type = DataType.DT_UI8;
                    break;
                case "STR":
                    type = DataType.DT_STR;
                    break;
                case "DATE":
                    type = DataType.DT_DBDATE;
                    break;
                case "TIME":
                    type = DataType.DT_DBTIME;
                    break;
                case "TIMESTAMP":
                    type = DataType.DT_DBTIMESTAMP;
                    break;
                default:
                    Message.Trace(Severity.Error, "Error in Type {0} - Unhandled VULCAN SSIS Type", typeAsString);
                    break;
            }
            return type;
        }
    }
}
