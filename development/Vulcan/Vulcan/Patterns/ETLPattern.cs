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
using Vulcan.Transformations;
using Vulcan.Emitters;
using Vulcan.Properties;
using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Vulcan.Patterns
{
    public class ETLPattern : Pattern
    {
        private List<Vulcan.Tasks.PrecedenceConstraint> pcList = new List<Vulcan.Tasks.PrecedenceConstraint>();
        public ETLPattern(Packages.VulcanPackage vulcanPackage, DTS.IDTSSequence parentContainer)
            :
            base(
            vulcanPackage,
            parentContainer
            )
        {
        }

        public override void Emit(XPathNavigator patternNavigator)
        {

            // Reloads invalidate the ParentContainer, so we should do it much later.
            string etlName =
                patternNavigator.SelectSingleNode("@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value;
            Message.Trace(Severity.Notification, "{0}", etlName);
            bool delayValidation = patternNavigator.SelectSingleNode("@DelayValidation").ValueAsBoolean;

            string sourceName = patternNavigator.SelectSingleNode("rc:SourceConnection/@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value;

            Connection sourceConnection =
                Connection.GetExistingConnection(
                                          VulcanPackage,
                                          patternNavigator.SelectSingleNode("rc:SourceConnection/@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value
                                        );

            if (sourceConnection != null)
            {
                string query = patternNavigator.SelectSingleNode("rc:Query", VulcanPackage.VulcanConfig.NamespaceManager).Value.Trim();

                // Add the Data Flow Task to the Package.
                DTS.TaskHost pipeHost = (DTS.TaskHost)ParentContainer.Executables.Add("STOCK:PipelineTask");
                pipeHost.Properties["DelayValidation"].SetValue(pipeHost, delayValidation);
                MainPipe dataFlowTask = (MainPipe)pipeHost.InnerObject;
                pipeHost.Name = etlName;

                // Add the Source (this is temporary and will be replaced with a new style of Source element)
                IDTSComponentMetaData90 sourceDataComponent = dataFlowTask.ComponentMetaDataCollection.New();
                sourceDataComponent.ComponentClassID = "DTSAdapter.OleDbSource.1";

                // IMPORTANT! If you do not Instantiate() and ProvideComponentProperties first, 
                // the component names do not get set... this is bad.
                CManagedComponentWrapper oleInstance = sourceDataComponent.Instantiate();
                oleInstance.ProvideComponentProperties();

                sourceDataComponent.Name = etlName + " Source";

                if (sourceDataComponent.RuntimeConnectionCollection.Count > 0)
                {
                    sourceDataComponent.RuntimeConnectionCollection[0].ConnectionManager =
                        DTS.DtsConvert.ToConnectionManager90(
                                                             sourceConnection.ConnectionManager
                                                             );
                    sourceDataComponent.RuntimeConnectionCollection[0].ConnectionManagerID =
                                                                                sourceConnection.ConnectionManager.ID;
                }

                oleInstance.SetComponentProperty("AccessMode", 2);
                oleInstance.SetComponentProperty("SqlCommand", query);

                try
                {
                    oleInstance.AcquireConnections(null);
                    oleInstance.ReinitializeMetaData();
                    oleInstance.ReleaseConnections();
                }
                catch (System.Runtime.InteropServices.COMException ce)
                {
                    Message.Trace(Severity.Error, ce, "OLEDBSource:{0}: {1}: Source {2}: Query {3}", sourceDataComponent.GetErrorDescription(ce.ErrorCode), ce.Message, sourceConnection.ConnectionManager.Name, query);
                }
                catch (Exception e)
                {
                    Message.Trace(Severity.Error, e, "OLEDBSource:{0}: Source {1}: Query {2}", e.Message, sourceConnection.ConnectionManager.Name, query);
                }

                //Map parameter variables:

                StringBuilder parameterBuilder = new StringBuilder();
                foreach (XPathNavigator paramNav in patternNavigator.Select("rc:Parameter", VulcanPackage.VulcanConfig.NamespaceManager))
                {
                    string name = paramNav.SelectSingleNode("@Name").Value;
                    string varName = paramNav.SelectSingleNode("@VariableName").Value;

                    if (VulcanPackage.DTSPackage.Variables.Contains(varName))
                    {
                        DTS.Variable variable = VulcanPackage.DTSPackage.Variables[varName];
                        parameterBuilder.AppendFormat("\"{0}\",{1};", name, variable.ID);
                    }
                    else
                    {
                        Message.Trace(Severity.Error, "DTS Variable {0} does not exist", varName);
                    }
                }

                oleInstance.SetComponentProperty("ParameterMapping", parameterBuilder.ToString());

                ///Transformation Factory
                IDTSComponentMetaData90 parentComponent = sourceDataComponent;
                XPathNavigator transNav = patternNavigator.SelectSingleNode("rc:Transformations", VulcanPackage.VulcanConfig.NamespaceManager);
                if (transNav != null)
                {
                    foreach (XPathNavigator nav in transNav.SelectChildren(XPathNodeType.Element))
                    {
                        // this is naughty but can be fixed later :)
                        Transformation t = TransformationFactory.ProcessTransformation(VulcanPackage, parentComponent, dataFlowTask, nav);
                        if (t != null)
                        {
                            parentComponent = t.Component;
                        }
                    }
                }

                XPathNavigator destNav = patternNavigator.SelectSingleNode("rc:Destination", VulcanPackage.VulcanConfig.NamespaceManager);
                if (destNav != null)
                {
                    string name = destNav.SelectSingleNode("@Name").Value;
                    Connection destConnection = Connection.GetExistingConnection(VulcanPackage, destNav.SelectSingleNode("@ConnectionName").Value);
                    string tableName = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", destNav.SelectSingleNode("@Table").Value.Trim());
                    OLEDBDestination oledbDestination = new OLEDBDestination(VulcanPackage, dataFlowTask, parentComponent, name, name, destConnection, tableName);

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
                            fastLoadOptions = fastLoadOptions.Replace(",", "",fastLoadOptions.Length-5,5);

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
                    foreach (XPathNavigator nav in destNav.Select("rc:Map", VulcanPackage.VulcanConfig.NamespaceManager))
                    {
                        string source = nav.SelectSingleNode("@Source").Value;
                        string destination;
                        bool unMap = false;

                        if (nav.SelectSingleNode("@Destination") == null)
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
                this.FirstExecutableGeneratedByPattern = pipeHost;
                this.LastExecutableGeneratedByPattern = pipeHost;
            } //END sourceConnection != null
            else
            {
                Message.Trace(Severity.Error, "Source Connection {0} does not exist in {1}", sourceName, etlName);
            }
        } //END function Emit
    } //END CLASS ETLPattern
}//END namespace 
