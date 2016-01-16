using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.IR.Framework;

namespace Ssis2008Emitter.Emitters.Framework
{
    public class SsisConnection
    {
        private DTS.ConnectionManager _connectionManager;

        // Get an existing connection
        public SsisConnection(string connectionName)
        {
            if (SsisPackage.CurrentPackage.DTSPackage.Connections.Contains(connectionName))
            {
                _connectionManager = SsisPackage.CurrentPackage.DTSPackage.Connections[connectionName];
            }
            else
            {
                MessageEngine.Global.Trace(Severity.Alert, Resources.ConnectionNotExists, connectionName);
            }
        }

        // Create a new connection or get an existing connection
        public SsisConnection(ConnectionConfiguration connectionConfiguration)
        {
            AddConnection(connectionConfiguration);
        }

        public DTS.ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
        }

        public virtual DTS.IDTSPropertiesProvider TaskHost
        {
            get { return _connectionManager; }
        }

        public virtual void SetExpression(string name, string expression)
        {
            TaskHost.SetExpression(name, expression);
        }

        public virtual void SetProperty(string name, object value)
        {
            if (TaskHost.Properties.Contains(name))
            {
                TaskHost.Properties[name].SetValue(TaskHost, value);
            }
        }

        private void AddConnection(ConnectionConfiguration connectionConfiguration)
        {
            if (SsisPackage.CurrentPackage.DTSPackage.Connections.Contains(connectionConfiguration.Name))
            {
                MessageEngine.Global.Trace(Severity.Alert, Resources.ConnectionNameConflicts, connectionConfiguration.Name);
                _connectionManager = SsisPackage.CurrentPackage.DTSPackage.Connections[connectionConfiguration.Name];
            }
            else
            {
                _connectionManager = SsisPackage.CurrentPackage.DTSPackage.Connections.Add(connectionConfiguration.Type);
                _connectionManager.Name = connectionConfiguration.Name;

                switch (connectionConfiguration.Type)
                {
                    case "OLEDB":
                        string connectionString = connectionConfiguration.ConnectionString;
                        SetProperty("RetainSameConnection", connectionConfiguration.RetainSameConnection);
                        SetProperty("ConnectionString", connectionString);
                        break;
                    case "FILE":
                        SetExpression("ConnectionString", connectionConfiguration.ConnectionString);
                        break;
                    default:
                        MessageEngine.Global.Trace(Severity.Error, Resources.ConnectionTypeNotImplemented, connectionConfiguration.Type);
                        break;
                }
            }
        }
    }
}
