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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Vulcan.Common;
using Vulcan.Tasks;
using Vulcan.Common.Templates;
using Vulcan.Emitters;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;

namespace Vulcan.Patterns
{ // NOTE! Most of this garbage is temporary while we're building the logical primitives for Stored Procs
    public class DimensionsPattern : Pattern
    {
        public DimensionsPattern(Vulcan.Packages.VulcanPackage vulcanPackage, DTS.IDTSSequence parentContainer)
            :
            base(
            vulcanPackage,
            parentContainer
            )
        {
        }

        public override void Emit(XPathNavigator patternNavigator)
        {
            if (patternNavigator != null)
            {

                string createTableFile = VulcanPackage.AddFileToProject(
                                                Resources.Create + Resources.Schema + Resources.Seperator +
                                                VulcanPackage.Name +
                                                Resources.ExtensionSQLFile
                                            );

                File.Delete(createTableFile);
                string insertSPFile = null;
                string checkAndinsertSPFile = null;
                string insertAndUpdateSPFile = null;
                bool hasCheckAndInsert = false;
                bool hasInsertAndUpdate = false;

                foreach (XPathNavigator nav in patternNavigator.Select("rc:Dimension", VulcanPackage.VulcanConfig.NamespaceManager))
                {
                    string dimensionName = nav.SelectSingleNode("@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value;

                    Message.Trace(Severity.Debug, "Emitting Create Statement for " + dimensionName);

                    TableHelper th = new TableHelper(dimensionName, VulcanPackage.VulcanConfig, nav.SelectSingleNode("rc:Table", VulcanPackage.VulcanConfig.NamespaceManager));
                    TableEmitterEx te = new TableEmitterEx(th, VulcanPackage);
                    te.Emit(createTableFile, true);

                    if (String.IsNullOrEmpty(insertSPFile))
                    {
                        insertSPFile = VulcanPackage.AddFileToProject(
                                                Resources.SPPrefix + Resources.Seperator +
                                                Resources.Insert +
                                                VulcanPackage.Name +
                                                Resources.ExtensionSQLFile
                                                );
                        File.Delete(insertSPFile);
                    }

                    InsertSPEmitter spe = new InsertSPEmitter(dimensionName, nav.SelectSingleNode("rc:Table", VulcanPackage.VulcanConfig.NamespaceManager), th, VulcanPackage);
                    spe.Emit(insertSPFile, true);

                    if (nav.Select("rc:Table/rc:CheckAndInsertUniqueColumn", VulcanPackage.VulcanConfig.NamespaceManager).Count > 0)
                    {
                        hasCheckAndInsert = true;


                        if (String.IsNullOrEmpty(checkAndinsertSPFile))
                        {
                            checkAndinsertSPFile = VulcanPackage.AddFileToProject(
                                                           Resources.SPPrefix + Resources.Seperator +
                                                           Resources.CheckAndInsert +
                                                           VulcanPackage.Name +
                                                           Resources.ExtensionSQLFile
                                                           );

                            File.Delete(checkAndinsertSPFile);
                        }

                        CheckAndInsertSPEmitter cispe = new CheckAndInsertSPEmitter(dimensionName, nav.SelectSingleNode("rc:Table", VulcanPackage.VulcanConfig.NamespaceManager), th, VulcanPackage);
                        cispe.Emit(checkAndinsertSPFile, true);
                    }

                    if (nav.Select("rc:Table/rc:InsertOrUpdateUniqueColumn", VulcanPackage.VulcanConfig.NamespaceManager).Count > 0)
                    {
                        hasInsertAndUpdate = true;

                        if (String.IsNullOrEmpty(insertAndUpdateSPFile))
                        {
                            insertAndUpdateSPFile = VulcanPackage.AddFileToProject(
                                                           Resources.SPPrefix + Resources.Seperator +
                                                           Resources.CheckAndInsert +
                                                           Resources.Update +
                                                           VulcanPackage.Name +
                                                           Resources.ExtensionSQLFile
                                                           );

                            File.Delete(insertAndUpdateSPFile);
                        }
                        InsertAndUpdateSPEmitter iuspe = new InsertAndUpdateSPEmitter(dimensionName, nav.SelectSingleNode("rc:Table", VulcanPackage.VulcanConfig.NamespaceManager), th, VulcanPackage);
                        iuspe.Emit(insertAndUpdateSPFile, true);
                    }
                }

                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties["Name"] = Resources.Create + Resources.Schema + Resources.Seperator + VulcanPackage.Name;

                //TODO: This is a hardcoded bug, should be removed when Dimensions are rewritten!
                Connection dimensionConnection = Connection.GetExistingConnection(VulcanPackage, patternNavigator);

                SQLTask createTableTask =
                    new SQLTask(
                                VulcanPackage,
                                properties["Name"].ToString(),
                                properties["Name"].ToString(),
                                ParentContainer,
                                dimensionConnection,
                                properties
                                );

                createTableTask.TransmuteToFileTask(properties["Name"] + Resources.ExtensionSQLFile);

                properties["Name"] = Resources.SPPrefix + Resources.Seperator +
                                        Resources.Insert +
                                        VulcanPackage.Name;

                this.FirstExecutableGeneratedByPattern = createTableTask.SQLTaskHost;
                this.LastExecutableGeneratedByPattern = this.FirstExecutableGeneratedByPattern;


                SQLTask createInsertSPTask =
                    new SQLTask(
                                VulcanPackage,
                                properties["Name"].ToString(),
                                properties["Name"].ToString(),
                                ParentContainer,
                                dimensionConnection,
                                properties
                                );
                createInsertSPTask.TransmuteToFileTask(properties["Name"] + Resources.ExtensionSQLFile);


                properties["Name"] = Resources.SPPrefix + Resources.Seperator +
                                        Resources.CheckAndInsert +
                                        VulcanPackage.Name;

                properties["Description"] = properties["Name"];

                VulcanPackage.AddPrecedenceConstraint(LastExecutableGeneratedByPattern, createInsertSPTask.SQLTaskHost, ParentContainer);
                this.LastExecutableGeneratedByPattern = createInsertSPTask.SQLTaskHost;

                if (hasCheckAndInsert)
                {
                    SQLTask createCheckInsertSPTask =
                        new SQLTask(
                                    VulcanPackage,
                                    properties["Name"].ToString(),
                                    properties["Name"].ToString(),
                                    ParentContainer,
                                    dimensionConnection,
                                    properties
                                    );
                    createCheckInsertSPTask.TransmuteToFileTask(properties["Name"] + Resources.ExtensionSQLFile);

                    properties["Name"] = Resources.SPPrefix + Resources.Seperator +
                                            Resources.Insert + Resources.Defaults +
                                            VulcanPackage.Name;

                    properties["Description"] = properties["Name"];

                    VulcanPackage.AddPrecedenceConstraint(createInsertSPTask.SQLTaskHost, createCheckInsertSPTask.SQLTaskHost, ParentContainer);
                    this.LastExecutableGeneratedByPattern = createCheckInsertSPTask.SQLTaskHost;
                } // hasCheckAndInsert

                if (hasInsertAndUpdate)
                {
                    properties["Name"] = properties["Name"] = Resources.SPPrefix + Resources.Seperator +
                                            Resources.CheckAndInsert + Resources.Update +
                                            VulcanPackage.Name;
                    properties["Description"] = properties["Name"];

                    SQLTask createInsertUpdateTask =
    new SQLTask(
                VulcanPackage,
                properties["Name"].ToString(),
                properties["Name"].ToString(),
                ParentContainer,
                dimensionConnection,
                properties
                );
                    createInsertUpdateTask.TransmuteToFileTask(properties["Name"] + Resources.ExtensionSQLFile);

                    VulcanPackage.AddPrecedenceConstraint(LastExecutableGeneratedByPattern, createInsertUpdateTask.SQLTaskHost, ParentContainer);
                    this.LastExecutableGeneratedByPattern = createInsertUpdateTask.SQLTaskHost;

                } // hasInsertAndUpdate
            }
        }

        public static Dictionary<string, XPathNavigator> GetDimensionsFromPackage(VulcanConfig vulcanConfig, XPathNavigator patternNavigator)
        {
            Dictionary<string, XPathNavigator> dimDict = new Dictionary<string, XPathNavigator>();
            foreach (XPathNavigator nav in patternNavigator.Select("rc:Dimension", vulcanConfig.NamespaceManager))
            {
                string dimensionName = nav.SelectSingleNode("@Name", vulcanConfig.NamespaceManager).Value;
                dimDict.Add(dimensionName, nav);
                Message.Trace(Severity.Debug, "Found Dimension " + dimensionName);
            }
            return dimDict;
        }
    }
}
