using System.Collections.Generic;
using System.Text;
using AstFramework.Model;
using AstLowerer.TSqlEmitter;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;

namespace AstLowerer.Capabilities
{
    public static class SchemaLowerer
    {
        public static void ProcessSchemas(AstRootNode astRootNode)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(astRootNode.SymbolTable);
            foreach (var astNamedNode in snapshotSymbolTable)
            {
                var schemaNode = astNamedNode as AstSchemaNode;
                if (schemaNode != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    ProcessSchema(astRootNode, schemaNode);
                }
            }
        }

        public static void ProcessSchema(AstRootNode astRootNode, AstSchemaNode schemaNode)
        {
            const string PackageTypeName = "Schema";

            var packageNode = new AstPackageNode(schemaNode.ParentItem);
            packageNode.Name = schemaNode.Name;
            packageNode.PackageType = PackageTypeName;

            var executeSqlNode = new AstExecuteSqlTaskNode(packageNode) { Name = schemaNode.Name, Connection = schemaNode.Connection, ResultSet = ExecuteSqlResultSet.None };
            executeSqlNode.Query = new AstExecuteSqlQueryNode(executeSqlNode) { QueryType = QueryType.Standard, Body = new TemplatePlatformEmitter("CreateSchema", schemaNode.Name).Emit() };

            packageNode.Tasks.Add(executeSqlNode);

            bool hasPermissions = false;
            var permissionBuilder = new StringBuilder();
            foreach (var permission in schemaNode.Permissions)
            {
                hasPermissions = true;
                permissionBuilder.AppendLine(PermissionsLowerer.ProcessPermission(schemaNode, permission));
            }

            if (hasPermissions)
            {
                var permissionsExecuteSqlTask = new AstExecuteSqlTaskNode(packageNode)
                {
                    Name = "__SetPermissions",
                    Connection = schemaNode.Connection,
                };
                permissionsExecuteSqlTask.Query = new AstExecuteSqlQueryNode(permissionsExecuteSqlTask)
                {
                    Body = permissionBuilder.ToString(),
                    QueryType = QueryType.Standard
                };
                packageNode.Tasks.Add(permissionsExecuteSqlTask);
            }

            if (schemaNode.CustomExtensions != null)
            {
                packageNode.Tasks.Add(schemaNode.CustomExtensions);
            }

            astRootNode.Packages.Add(packageNode);
        }
    }
}
