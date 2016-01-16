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
using Vulcan.Packages;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;

namespace Vulcan.Tasks
{
    public class Connection : Task
    {
        private DTS.ConnectionManager _connectionManager;
        
        public Connection(Packages.VulcanPackage vulcanPackage, string name, string description, string connectionType, string connectionString)
            :
            base(
            vulcanPackage,
            name,
            description,
            vulcanPackage.DTSPackage
            )
        {
            if (vulcanPackage.DTSPackage.Connections.Contains(name))
            {
                this._connectionManager = vulcanPackage.DTSPackage.Connections[name];
                return;
            }
            else
            {
                switch (connectionType.ToUpperInvariant())
                {
                    case "FILE":
                        _connectionManager = vulcanPackage.DTSPackage.Connections.Add("FILE");
                        SetExpression("ConnectionString", connectionString);
                        break;
                    case "FTP":
                        _connectionManager = vulcanPackage.DTSPackage.Connections.Add("FTP");
                        SetExpression("ConnectionString", connectionString);
                        break;
                    case "OLEDB":
                        _connectionManager = vulcanPackage.DTSPackage.Connections.Add("OLEDB");
                        SetProperty("ConnectionString",connectionString);
                        SetProperty("RetainSameConnection","True");
                        SetExpression("RetainSameConnection",@"true");
                        
                        break;
                    default:
                        Message.Trace(Severity.Error,"Only FILE and OLEDB connection types are implemented.");
                        break;
                }
                _connectionManager.Name = name;
                _connectionManager.Description = description;
            }
        }

        public static Connection GetExistingConnection(Packages.VulcanPackage vulcanPackage, XPathNavigator parentNavigator)
        {
            XPathNavigator connectionNav = 
                parentNavigator.SelectSingleNode("rc:Connection/@Name", vulcanPackage.VulcanConfig.NamespaceManager);

            if (connectionNav != null)
            {
                return Connection.GetExistingConnection(vulcanPackage, connectionNav.Value);
            }
            return null;
        }

        public static Connection GetExistingConnection(Packages.VulcanPackage vulcanPackage, string name)
        {
            if (vulcanPackage.DTSPackage.Connections.Contains(name))
            {
                return new Connection(vulcanPackage,name,name,null,null);
            }
            return null;
        }

        public override void SetProperty(string name, object value)
        {
            _connectionManager.Properties[name].SetValue(_connectionManager, value);
        }

        public override void SetExpression(string expressionName, string expressionValue)
        {
            _connectionManager.SetExpression(expressionName, expressionValue);
        }

        public DTS.ConnectionManager ConnectionManager
        {
            get
            {
                return this._connectionManager;
            }
        }
    }
}
