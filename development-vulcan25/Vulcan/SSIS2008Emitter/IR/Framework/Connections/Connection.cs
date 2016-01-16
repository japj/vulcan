using System;
using Ssis2008Emitter.IR.Common;
using VulcanEngine.IR.Ast.Connection;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Framework.Connections
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public abstract class Connection : PhysicalObject, IPhysicalPropertiesProvider
    {
        private DTS.ConnectionManager _connectionManager;

        protected string OriginalConnectionString { get; private set; }

        protected bool IsInitialized { get; set; }
        
        protected bool ReusedExisting { get; set; }

        public string ConnectionType { get; protected set; }

        public string ConnectionString { get; set; }

        public bool RetainSameConnection { get; set; }

        public DTS.ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
        }

        protected Connection(string name) : base(name)
        {
        }

        protected Connection(AstConnectionNode astNode) : base(astNode.Name) 
        {
            switch (astNode.ConnectionType)
            {
                case VulcanEngine.IR.Ast.Connection.ConnectionType.File:
                    ConnectionType = "{8527E0C4-1D7E-46B5-AC99-4AD36D172CB3}";
                    break;
                case VulcanEngine.IR.Ast.Connection.ConnectionType.OleDB:
                    ConnectionType = "{3BA51769-6C3C-46B2-85A1-81E58DB7DAE1}";
                    break;
                case VulcanEngine.IR.Ast.Connection.ConnectionType.AdoNet:
                    ConnectionType = "AdoNet";
                    break;
            }

            ConnectionString = astNode.OleConnectionString;
            OriginalConnectionString = astNode.OleConnectionString;
            RetainSameConnection = astNode.RetainSameConnection;
        }

        public DTS.IDTSPropertiesProvider PropertyProvider
        {
            get { return _connectionManager; }
        }

        public void SetProperty(string name, object value)
        {
            PropertyProvider.Properties[name].SetValue(_connectionManager, value);
        }

        public void SetExpression(string name, string expression)
        {
            // Validation: Expression length must be <= 4000 characters - SSIS 2008 CU#1 limitation
            PropertyProvider.SetExpression(name, expression);
        }

        public override void Initialize(SsisEmitterContext context)
        {
            if (!IsInitialized)
            {
                if (context.Package.DtsPackage.Connections.Contains(Name))
                {
                    _connectionManager = context.Package.DtsPackage.Connections[Name];
                    ReusedExisting = true;
                }
                else
                {
                    _connectionManager = context.Package.DtsPackage.Connections.Add(ConnectionType);
                    _connectionManager.Name = Name;
                    _connectionManager.ConnectionString = ConnectionString;
                }

                // Get reinitialized connection string.
                ConnectionString = _connectionManager.ConnectionString;
            }

            IsInitialized = true;
        }
        
        public override bool Equals(object obj)
        {
            var connectionNode = obj as Connection;
            if (connectionNode == null)
            {
                return false;
            }

            return Equals(connectionNode);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Implements static binding pattern for equals.")]
        public bool Equals(Connection connection)
        {
            return Name.Equals(connection.Name, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
