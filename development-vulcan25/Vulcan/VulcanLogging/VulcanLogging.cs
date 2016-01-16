using System;
using System.Collections.Generic;
using System.Globalization;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;

namespace VulcanLogging
{
    internal static class VulcanLogging
    {
        private static AstVariableNode packageName;
        private static AstVariableNode packageGuid;
        private static AstVariableNode parentPackageGuid;
        private static AstVariableNode source;
        private static AstVariableNode sourceID;
        private static AstVariableNode parentSourceID;
        private static AstVariableNode machineName;
        private static AstVariableNode userName;
        private static AstVariableNode patchedExecutionGuid;
        private static AstVariableNode executionGuid;
        private static AstVariableNode errorDescription;

        public static void ProcessPackages(AstRootNode astRootNode)
        {
            foreach (var package in astRootNode.Packages)
            {
                _processPackage(package);
            }

            _processMetadataTasks(astRootNode.SymbolTable);
        }

        private static string _generateScopedVariableName(VulcanEngine.IR.Ast.PersistentVariables.AstPersistentVariableNode astNode)
        {
            return String.Format(CultureInfo.InvariantCulture, "_{0}_ID", astNode.ScopedName.Replace(".", "_"));
        }

        private static void _processWriteMetadata(AstWritePersistentVariableNode astNode)
        {
            var workflowFragment = new List<AstTaskNode>();
            var parentContainer = (AstContainerTaskNode)astNode.ParentItem;

            var writePresistentVariableID = new AstVariableNode(parentContainer)
            {
                Name = _generateScopedVariableName(astNode.PersistentVariable),
                TypeCode = TypeCode.String,
                Value = String.Empty
            };
            parentContainer.Variables.Add(writePresistentVariableID);

            var execSqlwritePersistentVariable =
                new AstExecuteSqlTaskNode(parentContainer)
                {
                    Name = astNode.Name,
                    Connection = astNode.Connection,
                    ResultSet = ExecuteSqlResultSet.None
                };
            execSqlwritePersistentVariable.Query = new AstExecuteSqlQueryNode(execSqlwritePersistentVariable)
                                                       {
                                                           Body =
                                                               String.Format(
                                                               CultureInfo.InvariantCulture,
                                                               "\"EXECUTE [usp_VulcanLog_WriteMetadata] ? OUTPUT, '{0}', '\"+ (DT_WSTR, 2000)@[User::{1}] + \"', {2}\"",
                                                               astNode.PersistentVariable.ScopedName,
                                                               astNode.SourceVariable.Name,
                                                               Convert.ToInt32(astNode.Commit)),
                                                           QueryType = QueryType.Expression
                                                       };
            execSqlwritePersistentVariable.Query.Parameters.Add(
                new AstExecuteSqlParameterMappingTypeNode(execSqlwritePersistentVariable)
                {
                    Name = "0",
                    Variable = writePresistentVariableID,
                    Direction = Direction.Output,
                    Length = 255
                });
            workflowFragment.Add(execSqlwritePersistentVariable);

            parentContainer.Tasks.Replace(astNode, workflowFragment);
        }

        private static void _processCommitMetadata(AstCommitPersistentVariableNode astNode)
        {
            var workflowFragment = new List<AstTaskNode>();
            var parentContainer = (AstContainerTaskNode)astNode.ParentItem;
            var scopedVariableName = _generateScopedVariableName(astNode.PersistentVariable);

            bool insideScope = false;

            // TODO: bugbug - replace with the proper symbol table to search child scopes.  This is only a cursory check.
            foreach (AstVariableNode v in parentContainer.Variables)
            {
                if (v.Name.Equals(scopedVariableName))
                {
                    insideScope = true;
                    break;
                }
            }

            if (!insideScope)
            {
                MessageEngine.Trace(
                    Severity.Warning,
                    "LOG:MC01 Metadata Commit for {0} in task {1} may fail.  Metadata should be committed in the same container scope as the metadata write.",
                    astNode.PersistentVariable,
                    astNode.Name);
            }

            var execSQL =
                new AstExecuteSqlTaskNode(parentContainer)
                {
                    Name = astNode.Name,
                    Connection = astNode.Connection,
                    ResultSet = ExecuteSqlResultSet.None
                };
            execSQL.Query = new AstExecuteSqlQueryNode(execSQL)
                                {
                                    Body = String.Format(CultureInfo.InvariantCulture, "\"EXECUTE [usp_VulcanLog_CommitMetadata] '\"+ (DT_WSTR, 255)@[User::{0}] + \"'\"", scopedVariableName),
                                    QueryType = QueryType.Expression
                                };
            workflowFragment.Add(execSQL);
            parentContainer.Tasks.Replace(astNode, workflowFragment);
        }

        private static void _processReadMetadata(AstReadPersistentVariableNode astNode)
        {
            var workflowFragment = new List<AstTaskNode>();
            var parentContainer = (AstContainerTaskNode)astNode.ParentItem;

            var execSQL =
                new AstExecuteSqlTaskNode(parentContainer)
                {
                    Name = astNode.Name,
                    Connection = astNode.Connection,
                    ResultSet = ExecuteSqlResultSet.None
                };
            execSQL.Query = new AstExecuteSqlQueryNode(execSQL)
                                {
                                    Body = String.Format(CultureInfo.InvariantCulture, "\"EXECUTE [usp_VulcanLog_ReadMetadata] ? OUTPUT, '{0}'\"", astNode.PersistentVariable.ScopedName),
                                    QueryType = QueryType.Expression
                                };
            execSQL.Query.Parameters.Add(
                new AstExecuteSqlParameterMappingTypeNode(execSQL)
                    {
                        Name = "0",
                        Variable = astNode.TargetVariable,
                        Direction = Direction.Output,
                        Length = 9999
                    });

            workflowFragment.Add(execSQL);
            parentContainer.Tasks.Replace(astNode, workflowFragment);
        }

        private static void _processMetadataTasks(SymbolTable symbolTable)
        {
            var snapshotSymbolTable = new List<IReferenceableItem>(symbolTable);
            foreach (var astNode in snapshotSymbolTable)
            {
                var writePersistent = astNode as AstWritePersistentVariableNode;

                if (writePersistent != null)
                {
                    _processWriteMetadata(writePersistent);
                }
                else
                {
                    var commitPersistent = astNode as AstCommitPersistentVariableNode;
                    if (commitPersistent != null)
                    {
                        _processCommitMetadata(commitPersistent);
                    }
                    else
                    {
                        var readPersistent = astNode as AstReadPersistentVariableNode;
                        if (readPersistent != null)
                        {
                            _processReadMetadata(readPersistent);
                        }
                    }
                }
            }
        }

        private static void _processPackage(AstPackageBaseNode package)
        {
            packageName = new AstVariableNode(package) { Name = "PackageName", IsSystemVariable = true, TypeCode = TypeCode.String };
            packageGuid = new AstVariableNode(package) { Name = "PackageID", IsSystemVariable = true, TypeCode = TypeCode.String };
            parentPackageGuid = new AstVariableNode(package) { Name = "_parentPackageGuid", TypeCode = TypeCode.String, Value = String.Empty, InheritFromPackageParentConfigurationString = "System::PackageID" };
            source = new AstVariableNode(package) { Name = "SourceName", TypeCode = TypeCode.String, IsSystemVariable = true };
            sourceID = new AstVariableNode(package) { Name = "SourceID", TypeCode = TypeCode.String, IsSystemVariable = true };
            parentSourceID = new AstVariableNode(package) { Name = "ParentContainerGUID", TypeCode = TypeCode.String, IsSystemVariable = true };
            machineName = new AstVariableNode(package) { Name = "MachineName", TypeCode = TypeCode.String, IsSystemVariable = true };
            userName = new AstVariableNode(package) { Name = "UserName", TypeCode = TypeCode.String, IsSystemVariable = true };
            patchedExecutionGuid = new AstVariableNode(package) { Name = "_patchedExecutionGuid", TypeCode = TypeCode.String, Value = "@[User::_executionGuid] == \"\" ? @[System::ExecutionInstanceGUID] : @[User::_executionGuid]", EvaluateAsExpression = true };
            executionGuid = new AstVariableNode(package) { Name = "_executionGuid", TypeCode = TypeCode.String, Value = String.Empty, InheritFromPackageParentConfigurationString = "User::_patchedExecutionGuid" };
            errorDescription = new AstVariableNode(package) { Name = "ErrorDescription", TypeCode = TypeCode.String, IsSystemVariable = true };

            package.Variables.Add(executionGuid);
            package.Variables.Add(parentPackageGuid);
            package.Variables.Add(patchedExecutionGuid);

            if (package.LogConnection != null)
            {
                _packageBuildOnPreExecuteEvent(package);
                _packageBuildOnErrorEvent(package);
                _packageBuildOnEndEvent(package);
            }
        }

        /// <summary>
        /// Container Find Event finds the Event Handler for a specific type if one exists.
        /// </summary>
        /// <param name="eventHandlerNodes">List of event handler nodes for this container.</param>
        /// <param name="eventType">Type of event to find.</param>
        /// <returns>ASTTaskEventHandlerNode of the EventHandler found</returns>
        private static AstTaskEventHandlerNode ContainerFindEvent(IEnumerable<AstTaskEventHandlerNode> eventHandlerNodes, EventType eventType)
        {
            foreach (var eventHandler in eventHandlerNodes)
            {
                if (eventHandler.EventType == eventType)
                {
                    return eventHandler;
                }
            }

            return null;
        }

        private static void _packageBuildOnErrorEvent(AstPackageBaseNode package)
        {
            var packageEvent = ContainerFindEvent(package.Events, EventType.OnError);

            if (packageEvent == null)
            {
                packageEvent = new AstTaskEventHandlerNode(package) { EventType = EventType.OnError };
                package.Events.Add(packageEvent);
            }

            var executeSql = new AstExecuteSqlTaskNode(packageEvent)
                                     {
                                         Name = "Exec usp_PackageError",
                                         ExecuteDuringDesignTime = false,
                                         Connection = package.LogConnection
                                     };
            executeSql.Query = new AstExecuteSqlQueryNode(executeSql)
                                   {
                                       Body = "\"EXECUTE [usp_PackageError] ?,?,?,?,?,?,?,?,?,? \"",
                                       QueryType = QueryType.Expression
                                   };

            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) {Name = "0", Direction = Direction.Input, Variable = packageName,Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "1", Direction = Direction.Input, Variable = packageGuid, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "2", Direction = Direction.Input, Variable = parentPackageGuid, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "3", Direction = Direction.Input, Variable = source, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "4", Direction = Direction.Input, Variable = sourceID, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "5", Direction = Direction.Input, Variable = parentSourceID, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "6", Direction = Direction.Input, Variable = machineName, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "7", Direction = Direction.Input, Variable = userName, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "8", Direction = Direction.Input, Variable = patchedExecutionGuid, Length = 255 });
            executeSql.Query.Parameters.Add(new AstExecuteSqlParameterMappingTypeNode(executeSql.Query) { Name = "9", Direction = Direction.Input, Variable = errorDescription, Length = -1 });

            packageEvent.Tasks.Insert(0, executeSql);
        }

        private static void _packageBuildOnEndEvent(AstPackageBaseNode package)
        {
            var packageEvent = ContainerFindEvent(package.Events, EventType.OnPostExecute);

            if (packageEvent == null)
            {
                packageEvent = new AstTaskEventHandlerNode(package) { EventType = EventType.OnPostExecute };
                package.Events.Add(packageEvent);
            }

            var postExecContainer = new AstContainerTaskNode(packageEvent) { Name = "OnPostExec", ConstraintMode = ContainerConstraintMode.Linear };
            packageEvent.Tasks.Insert(0, postExecContainer);

            var constrainedCont = new AstContainerTaskNode(packageEvent) { Name = "ConstrainedContainer", ConstraintMode = ContainerConstraintMode.Parallel };
            postExecContainer.Tasks.Add(constrainedCont);

            var executeSql = new AstExecuteSqlTaskNode(postExecContainer)
                                 {
                                     Name = "Exec usp_PackageEnd",
                                     ExecuteDuringDesignTime = false,
                                     Connection = package.LogConnection
                                 };
            executeSql.PrecedenceConstraints = new AstTaskflowPrecedenceConstraintsNode(executeSql);
            executeSql.PrecedenceConstraints.Inputs.Add(
                new AstTaskflowInputPathNode(executeSql.PrecedenceConstraints)
                {
                    Expression = "@[System::PackageID] == @[System::SourceID]",
                    EvaluationOperation = TaskEvaluationOperationType.Expression,
                    OutputPath = constrainedCont.OutputPath
                });
            executeSql.Query = new AstExecuteSqlQueryNode(executeSql)
                                   {
                                       Body = "\"EXEC usp_PackageEnd \" + (@[System::PackageName] == \"\" ? \"NULL\" : \"'\"+@[System::PackageName]+\"'\") +\",\"+(@[System::PackageID] == \"\" ? \"NULL\" : \"'\"+@[System::PackageID]+\"'\") +\",\"+(@[User::_parentPackageGuid] == \"\" ? \"NULL\" : \"'\"+@[User::_parentPackageGuid]+\"'\") +\",\"+(@[System::SourceName] == \"\" ? \"NULL\" : \"'\"+@[System::SourceName]+\"'\") +\",\"+(@[System::SourceID] == \"\" ? \"NULL\" : \"'\"+@[System::SourceID]+\"'\") +\",\"+(@[System::SourceParentGUID] == \"\" ? \"NULL\" : \"'\"+@[System::SourceParentGUID]+\"'\") +\",\"+ (@[System::MachineName] == \"\" ? \"NULL\" : \"'\"+@[System::MachineName]+\"'\") +\",\"+(@[System::UserName] == \"\" ? \"NULL\" : \"'\"+@[System::UserName]+\"'\") +\",\"+(@[User::_patchedExecutionGuid] == \"\" ? \"NULL\" : \"'\"+@[User::_patchedExecutionGuid])+\"'\"",
                                       QueryType = QueryType.Expression
                                   };
            postExecContainer.Tasks.Add(executeSql);
        }

        private static void _packageBuildOnPreExecuteEvent(AstPackageBaseNode package)
        {
            var packageEvent = ContainerFindEvent(package.Events, EventType.OnPreExecute);

            if (packageEvent == null)
            {
                packageEvent = new AstTaskEventHandlerNode(package) { EventType = EventType.OnPreExecute };
                package.Events.Add(packageEvent);
            }

            var preExecContainer = new AstContainerTaskNode(packageEvent) { Name = "OnPreExec", ConstraintMode = ContainerConstraintMode.Linear };
            packageEvent.Tasks.Insert(0, preExecContainer);

            var constrainedCont = new AstContainerTaskNode(packageEvent) { Name = "ConstrainedContainer", ConstraintMode = ContainerConstraintMode.Parallel };
            preExecContainer.Tasks.Add(constrainedCont);

            var executeSql = new AstExecuteSqlTaskNode(preExecContainer)
                                 {
                                     Name = "Exec usp_PackageStart",
                                     ExecuteDuringDesignTime = false,
                                     Connection = package.LogConnection
                                 };
            executeSql.PrecedenceConstraints = new AstTaskflowPrecedenceConstraintsNode(executeSql);
            executeSql.PrecedenceConstraints.Inputs.Add(
                new AstTaskflowInputPathNode(executeSql.PrecedenceConstraints)
                {
                    Expression = "@[System::PackageID] == @[System::SourceID]",
                    EvaluationOperation = TaskEvaluationOperationType.Expression,
                    OutputPath = constrainedCont.OutputPath
                });
            executeSql.Query = new AstExecuteSqlQueryNode(executeSql)
                                   {
                                       Body = "\"EXEC usp_PackageStart \" + (@[System::PackageName] == \"\" ? \"NULL\" : \"'\"+@[System::PackageName]+\"'\") +\",\"+(@[System::PackageID] == \"\" ? \"NULL\" : \"'\"+@[System::PackageID]+\"'\") +\",\"+(@[User::_parentPackageGuid] == \"\" ? \"NULL\" : \"'\"+@[User::_parentPackageGuid]+\"'\") +\",\"+(@[System::SourceName] == \"\" ? \"NULL\" : \"'\"+@[System::SourceName]+\"'\") +\",\"+(@[System::SourceID] == \"\" ? \"NULL\" : \"'\"+@[System::SourceID]+\"'\") +\",\"+(@[System::SourceParentGUID] == \"\" ? \"NULL\" : \"'\"+@[System::SourceParentGUID]+\"'\") +\",\"+ (@[System::MachineName] == \"\" ? \"NULL\" : \"'\"+@[System::MachineName]+\"'\") +\",\"+(@[System::UserName] == \"\" ? \"NULL\" : \"'\"+@[System::UserName]+\"'\") +\",\"+(@[User::_patchedExecutionGuid] == \"\" ? \"NULL\" : \"'\"+@[User::_patchedExecutionGuid])+\"'\"",
                                       QueryType = QueryType.Expression
                                   };
            preExecContainer.Tasks.Add(executeSql);
        }
    }
}
