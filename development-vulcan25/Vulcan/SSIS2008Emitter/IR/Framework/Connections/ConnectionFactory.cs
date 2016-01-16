using System;
using System.Globalization;
using VulcanEngine.IR.Ast.Connection;

namespace Ssis2008Emitter.IR.Framework.Connections
{
    public static class ConnectionFactory
    {
        public static Connection CreateConnection(AstConnectionNode astNode)
        {
            switch (astNode.ConnectionType)
            {
                case ConnectionType.OleDB: return new OleDBConnection(astNode);
                case ConnectionType.AdoNet: return new AdoNetConnection(astNode);
                case ConnectionType.File: return new FileConnection(astNode.Name, astNode.OleConnectionString);
                default:
                    throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ErrorEnumerationTypeNotSupported, astNode.ConnectionType));
            }
        }
    }
}