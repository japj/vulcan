using System;
using System.Globalization;
using AstFramework;
using AstLowerer.TSqlEmitter;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Task;

namespace AstLowerer.Capabilities
{
    public static class PrincipalsLowerer
    {
        public static void ProcessPrincipals(AstRootNode astRootNode)
        {
            if (astRootNode.Principals.Count > 0)
            {
                var package = new AstPackageNode(astRootNode) { Name = "PrincipalsInitializer", Emit = true, PackageType = "Principal" };
                astRootNode.Packages.Add(package);

                foreach (AstPrincipalNode principal in astRootNode.Principals)
                {
                    TemplatePlatformEmitter principalTemplate;
                    switch (principal.PrincipalType)
                    {
                        case PrincipalType.ApplicationRole:
                            principalTemplate = new TemplatePlatformEmitter("CreatePrincipalApplicationRole", principal.Name);
                            break;
                        case PrincipalType.DBRole:
                            principalTemplate = new TemplatePlatformEmitter("CreatePrincipalDatabaseRole", principal.Name);
                            break;
                        case PrincipalType.SqlUser:
                            principalTemplate = new TemplatePlatformEmitter("CreatePrincipalSqlUser", principal.Name);
                            break;
                        case PrincipalType.WindowsGroup:
                            principalTemplate = new TemplatePlatformEmitter("CreatePrincipalWindowsUser", principal.Name);
                            break;
                        case PrincipalType.WindowsUser:
                            principalTemplate = new TemplatePlatformEmitter("CreatePrincipalWindowsUser", principal.Name);
                            break;
                        default:
                            MessageEngine.Trace(principal, Severity.Error, "V0139", "Unknown Principal Type {0} in principal {1}", principal.PrincipalType.ToString(), principal.Name);
                            return;
                    }

                    var executeSqlTask = new AstExecuteSqlTaskNode(package)
                    {
                        Name = Utility.NameCleanerAndUniqifier(String.Format(CultureInfo.InvariantCulture, "PrincipalConfig_{0}", principal.Name)),
                        Connection = principal.Connection
                    };
                    executeSqlTask.Query = new AstExecuteSqlQueryNode(executeSqlTask)
                                               {
                                                   Body = principalTemplate.Emit(),
                                                   QueryType = QueryType.Standard
                                               };
                    package.Tasks.Add(executeSqlTask);
                }
            }
        }
    }
}
