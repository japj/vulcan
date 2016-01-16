using System.ComponentModel;
using AstLowerer.TSqlEmitter;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Table;

namespace AstLowerer.Capabilities
{
    public static class PermissionsLowerer
    {
        public static string ProcessPermission(AstSecurableNode securable, AstPermissionNode permission)
        {
            var table = securable as AstTableNode;
            var column = securable as AstTableColumnBaseNode;
            var schema = securable as AstSchemaNode;

            var action = TypeDescriptor.GetConverter(permission.Action).ConvertTo(permission.Action, typeof(string)) as string;
            var target = TypeDescriptor.GetConverter(permission.Target).ConvertTo(permission.Target, typeof(string)) as string;

            if (table != null)
            {
                return new TemplatePlatformEmitter("CreateTablePermission", action, target, table.SchemaQualifiedName, permission.Principal.Name).Emit();
            }
            
            if (column != null)
            {
                var columnTable = column.ParentItem as AstTableNode;
                if (columnTable != null)
                {
                    return new TemplatePlatformEmitter("CreateTableColumnPermission", action, target, columnTable.SchemaQualifiedName, column.Name, permission.Principal.Name).Emit();
                }
            }
            
            if (schema != null)
            {
                return new TemplatePlatformEmitter("CreateSchemaPermission", action, target, schema.Name, permission.Principal.Name).Emit();
            }

            return null;
        }
    }
}
