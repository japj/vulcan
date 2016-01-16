using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TSQLEmitter = Ssis2008Emitter.Emitters.TSQL.PlatformEmitter;

using VulcanEngine.Common;

using VulcanEngine.IR.Ast;
using AstConnection = VulcanEngine.IR.Ast.Connection;
using AstCube = VulcanEngine.IR.Ast.Cube;
using AstDimension = VulcanEngine.IR.Ast.Dimension;
using AstDimensionInstance = VulcanEngine.IR.Ast.DimensionInstance;
using AstFact = VulcanEngine.IR.Ast.Fact;
using AstMeasureGroup = VulcanEngine.IR.Ast.MeasureGroup;
using AstTable = VulcanEngine.IR.Ast.Table;
using AstTask = VulcanEngine.IR.Ast.Task;
using AstTransformation = VulcanEngine.IR.Ast.Transformation;

using Ssis2008Emitter.Properties;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using PhysicalTask = Ssis2008Emitter.IR.Task;
using PhysicalDataFlow = Ssis2008Emitter.IR.DataFlow;
using PhysicalTSQL = Ssis2008Emitter.IR.TSQL;


namespace Ssis2008Emitter.Utility
{
    public static class AstLoweringExtensionMethods
    {

        private static Package _CurrentPackage = null;
        #region Package
        public static Package Lower(this AstTask.AstPackageNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                Package package = new Package();
                _CurrentPackage = package;
                package.ConstraintMode = astNode.ConstraintMode.ToString();
                package.DefaultPlatform = astNode.DefaultPlatform.ToString();
                package.Log = astNode.Log;
                AddConnection(astNode.LogConnection);
                package.LogConnectionName = astNode.LogConnection == null ? null : astNode.LogConnection.Name;
                package.Name = astNode.Name;
                package.Type = astNode.Type.ToString();

                // fix this so packages lower themselves via Sequence LoweringLayer in the friday refactor
                switch (astNode.TransactionMode)
                {
                    case VulcanEngine.IR.Ast.Task.ContainerTransactionMode.StartOrJoin:
                        package.TransactionMode = "Required";
                        break;
                    case VulcanEngine.IR.Ast.Task.ContainerTransactionMode.Join:
                        package.TransactionMode = "Supported";
                        break;
                    case VulcanEngine.IR.Ast.Task.ContainerTransactionMode.NoTransaction:
                        package.TransactionMode = "NotSupported";
                        break;
                    default:
                        package.TransactionMode = "Supported";
                        break;
                }

                foreach (AstTask.AstVariableNode variableNode in astNode.Variables)
                {
                    Variable physicalVariableNode = variableNode.Lower();
                    physicalVariableNode.Parent = package;
                    package.VariableList.Add(physicalVariableNode);
                }

                foreach (AstTask.AstTaskNode taskNode in astNode.Tasks)
                {
                    PhysicalTask.Task physicalTaskNode = taskNode.Lower();
                    if (physicalTaskNode != null)
                    {
                        physicalTaskNode.Parent = package;
                        package.Tasks.Add(physicalTaskNode);
                    }
                }

                ProcessHelperTables((AstTask.AstContainerTaskNode)astNode, package);

                return package;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        private static void AddConnection(AstConnection.AstConnectionNode connectionNode)
        {
            if (connectionNode != null)
            {
                ConnectionConfiguration connectionConfiguration = connectionNode.Lower();
                AddConnection(connectionConfiguration);
            }
        }

        private static void AddConnection(ConnectionConfiguration connectionConfiguration)
        {
            if (connectionConfiguration != null)
            {
                if (!_CurrentPackage.ConnectionConfigurationList.Contains(connectionConfiguration))
                {
                    _CurrentPackage.ConnectionConfigurationList.Add(connectionConfiguration);
                }
            }
        }


        #endregion  // Package

        #region Connection
        public static ConnectionConfiguration Lower(this AstConnection.AstConnectionNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }
            try
            {
                ConnectionConfiguration physicalConnection = new ConnectionConfiguration();
                physicalConnection.Name = astNode.Name;
                physicalConnection.ConnectionString = astNode.ConnectionString;
                physicalConnection.RetainSameConnection = astNode.RetainSameConnection;
                physicalConnection.Type = astNode.Type.ToString();
                physicalConnection.Parent = _CurrentPackage;
                return physicalConnection;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }
        #endregion  // Connection

        #region Dimension
        public static PhysicalTSQL.Table Lower(this AstDimension.AstDimensionNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            return ((AstTable.AstTableNode)astNode).Lower();

            /*
            foreach (AstDimension.AstAttributeNode attribute in astNode.Attributes)
            {
                List<AstDimension.AstAttributeColumnNode> columns = new List<VulcanEngine.IR.Ast.Dimension.AstAttributeColumnNode>(attribute.KeyColumns);
                if (attribute.NameColumn != null) { columns.Add(attribute.NameColumn); }
                if (attribute.ValueColumn != null) { columns.Add(attribute.ValueColumn); }
                foreach (AstDimension.AstAttributeColumnNode column in columns)
                {
                    PhysicalTSQL.Column physicalColumn = new Ssis2008Emitter.IR.TSQL.Column();
                    physicalColumn.Name = column.Name;
                    ///TODO: vsabella: put in custom parser
                    physicalColumn.Type = Ssis2008Emitter.Emitters.TSQL.PhysicalTypeTranslator.Translate(column.Type, column.Length, column.Precision, column.Scale, column.CustomType);
                    physicalColumn.IsNullable = column.IsNullable;
                    physicalColumn.Default = column.Default;
                    table.Columns.ColumnList.Add(physicalColumn);
                }
            }
            return table;*/
        }
        #endregion  // Dimension

        #region Fact
        public static PhysicalTSQL.Table Lower(this AstFact.AstFactNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            return ((AstTable.AstTableNode)astNode).Lower();
        }
        #endregion  // Fact

        #region Table

        private static void ProcessStaticSources(AstTable.AstTableNode astNode, PhysicalTSQL.Table table)
        {
            if (astNode.AsClassOnly)
            {
                return;
            }

            if (astNode.Sources != null)
            {
                foreach (AstTable.AstTableStaticSourceNode staticSource in astNode.Sources.OfType<AstTable.AstTableStaticSourceNode>())
                {
                    foreach (string row in staticSource.Rows)
                    {
                        Ssis2008Emitter.IR.TSQL.DefaultValue d = new Ssis2008Emitter.IR.TSQL.DefaultValue();
                        d.Value = row;
                        table.DefaultValues.DefaultValueList.Add(d);
                    }
                }
            }
        }
        public static PhysicalTSQL.Table Lower(this AstTable.AstTableNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                PhysicalTSQL.Table table = new Ssis2008Emitter.IR.TSQL.Table();
                table.Name = astNode.Name;
                table.Columns.Parent = table;
                table.Indexes.Parent = table;

                ProcessStaticSources(astNode, table);
                table.ConnectionConfiguration = astNode.Connection != null ? astNode.Connection.Lower() : null;

                foreach (AstTable.AstTableColumnBaseNode columnBase in astNode.Columns)
                {
                    ProcessAstTableColumnBaseNode(table, columnBase);
                }

                // TODO: Strip out UniqueKey if it is identical to keyPhysicalConstraint above
                foreach (AstTable.AstTableKeyBaseNode keyBase in astNode.Keys)
                {
                    PhysicalTSQL.Constraint keyPhysicalConstraint = null;
                    if (keyBase is AstTable.AstTablePrimaryKeyNode)
                    {
                        keyPhysicalConstraint = new Ssis2008Emitter.IR.TSQL.PrimaryKeyConstraint();
                    }
                    else if (keyBase is AstTable.AstTableIdentityNode)
                    {
                        PhysicalTSQL.IdentityConstraint physicalIdentity = new Ssis2008Emitter.IR.TSQL.IdentityConstraint();
                        physicalIdentity.Seed = ((AstTable.AstTableIdentityNode)keyBase).Seed;
                        physicalIdentity.Increment = ((AstTable.AstTableIdentityNode)keyBase).Increment;
                        keyPhysicalConstraint = physicalIdentity;
                    }
                    else
                    {
                        keyPhysicalConstraint = new Ssis2008Emitter.IR.TSQL.Constraint();
                    }

                    if (keyPhysicalConstraint != null)
                    {
                        keyPhysicalConstraint.Clustered = keyBase.Clustered;
                        keyPhysicalConstraint.DropExisting = keyBase.DropExisting;
                        keyPhysicalConstraint.IgnoreDupKey = keyBase.IgnoreDupKey;
                        keyPhysicalConstraint.Name = keyBase.Name;
                        keyPhysicalConstraint.Online = keyBase.Online;
                        keyPhysicalConstraint.PadIndex = keyBase.PadIndex;
                        keyPhysicalConstraint.Parent = table;
                        keyPhysicalConstraint.SortInTempdb = keyBase.SortInTempdb;
                        keyPhysicalConstraint.Unique = keyBase.Unique;
                        foreach (AstTable.AstTableKeyColumnNode key in keyBase.Columns)
                        {
                            PhysicalTSQL.Key physicalKey = new Ssis2008Emitter.IR.TSQL.Key();
                            physicalKey.Name = key.Column.Name;
                            keyPhysicalConstraint.Keys.Add(physicalKey);
                        }
                        table.ConstraintList.Add(keyPhysicalConstraint);
                    }
                }

                foreach (AstTable.AstTableForeignKeyNode foreignKey in astNode.ForeignKeys)
                {
                    PhysicalTSQL.ForeignKeyConstraint physicalForeignKey = new Ssis2008Emitter.IR.TSQL.ForeignKeyConstraint();
                    foreach (AstTable.AstTableForeignKeyColumnNode column in foreignKey.Columns)
                    {
                        PhysicalTSQL.Key localColumn = new Ssis2008Emitter.IR.TSQL.Key();
                        localColumn.Name = column.OutputName;
                        physicalForeignKey.LocalColumnList.Add(localColumn);
                        PhysicalTSQL.Key foreignColumn = new Ssis2008Emitter.IR.TSQL.Key();
                        foreignColumn.Name = column.ColumnName;
                        physicalForeignKey.ForeignColumnList.Add(foreignColumn);
                    }
                    physicalForeignKey.Table = foreignKey.Dimension.Name;
                    physicalForeignKey.Name = foreignKey.Name;
                    table.AddForeignKeyConstraint(physicalForeignKey);
                }

                foreach (AstTable.AstTableIndexNode index in astNode.Indexes)
                {
                    PhysicalTSQL.Index physicalIndex = new Ssis2008Emitter.IR.TSQL.Index();
                    physicalIndex.Clustered = index.Clustered;
                    physicalIndex.DropExisting = index.DropExisting;
                    physicalIndex.IgnoreDupKey = index.IgnoreDupKey;
                    physicalIndex.Name = index.Name;
                    physicalIndex.Online = index.Online;
                    physicalIndex.PadIndex = index.PadIndex;
                    physicalIndex.Parent = table;
                    physicalIndex.SortInTempdb = index.SortInTempdb;
                    physicalIndex.Unique = index.Unique;

                    foreach (AstTable.AstTableIndexColumnNode key in index.Columns)
                    {
                        PhysicalTSQL.Key physicalKey = new Ssis2008Emitter.IR.TSQL.Key();
                        physicalKey.Name = key.Column.Name;
                        physicalIndex.Keys.Add(physicalKey);
                    }
                    foreach (AstTable.AstTableColumnBaseNode leaf in index.Leafs)
                    {
                        PhysicalTSQL.Leaf physicalLeaf = new Ssis2008Emitter.IR.TSQL.Leaf();
                        physicalLeaf.Name = leaf.Name;
                        physicalIndex.Leaves.Add(physicalLeaf);
                    }

                    table.Indexes.IndexList.Add(physicalIndex);
                }
                //TODO: table.Tags;

                return table;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        private static void ProcessAstTableColumnBaseNode(PhysicalTSQL.Table table, AstTable.AstTableColumnBaseNode columnBase)
        {
            if (columnBase.AsClassOnly)
            {
                return;
            }

            try
            {
                AstDimension.AstAttributeColumnNode attributeColumn = new AstDimension.AstAttributeColumnNode();
                attributeColumn.Column = columnBase;
                AstTable.AstTableColumnBaseNode column = columnBase;
                AstTable.AstTableDimensionReferenceNode dimReference = columnBase as AstTable.AstTableDimensionReferenceNode;
                AstTable.AstTableHashedKeyColumnNode hashKey = columnBase as AstTable.AstTableHashedKeyColumnNode;

                if (hashKey != null)
                {
                    StringBuilder hashBytesBuilder = new StringBuilder();
                    foreach (AstTable.AstTableKeyColumnNode keyColumn in hashKey.Constraint.Columns)
                    {
                        string expression = "+ HASHBYTES('SHA1',{0})";
                        switch (keyColumn.Column.Type)
                        {
                            case VulcanEngine.IR.Ast.Table.ColumnType.WSTR:
                                expression = String.Format(expression, String.Format("UPPER(RTRIM(LTRIM(COALESCE({0},''))))", keyColumn.Column.Name));
                                break;
                            case VulcanEngine.IR.Ast.Table.ColumnType.STR:
                                expression = String.Format(expression, String.Format("UPPER(RTRIM(LTRIM(COALESCE({0},''))))", keyColumn.Column.Name));
                                break;
                            case VulcanEngine.IR.Ast.Table.ColumnType.INT32:
                            case VulcanEngine.IR.Ast.Table.ColumnType.INT64:
                            case VulcanEngine.IR.Ast.Table.ColumnType.UINT32:
                            case VulcanEngine.IR.Ast.Table.ColumnType.UINT64:
                                expression = String.Format(expression, String.Format("CONVERT(binary varying(64),COALESCE({0},-1))", keyColumn.Column.Name));
                                break;
                            default:
                                expression = String.Format(expression, keyColumn.Column.Name);
                                break;
                        }
                        hashBytesBuilder.Append(expression);
                    }


                    string hashExpression = String.Format("(CONVERT(binary varying(32),HASHBYTES('SHA1',{0})))", hashBytesBuilder.ToString().Substring(1));
                    PhysicalTSQL.Column physicalColumn = new Ssis2008Emitter.IR.TSQL.Column();
                    physicalColumn.Name = hashKey.Name;

                    physicalColumn.Type = "BINARY VARYING(32)";
                    physicalColumn.IsNullable = hashKey.IsNullable;
                    physicalColumn.Computed = String.Format("AS {0} PERSISTED NOT NULL UNIQUE", hashExpression);
                    physicalColumn.IsAssignable = hashKey.IsAssignable;

                    table.Columns.ColumnList.Add(physicalColumn);
                }

                if (attributeColumn != null)
                {
                    column = attributeColumn.Column;
                }

                if (column != null && hashKey == null && dimReference == null)
                {
                    PhysicalTSQL.Column physicalColumn = new Ssis2008Emitter.IR.TSQL.Column();
                    physicalColumn.Name = column.Name;

                    physicalColumn.Type = Ssis2008Emitter.Emitters.TSQL.PhysicalTypeTranslator.Translate(column.Type, column.Length, column.Precision, column.Scale, column.CustomType);
                    physicalColumn.IsNullable = column.IsNullable;
                    physicalColumn.Default = column.Default;

                    physicalColumn.Computed = column.Computed;
                    physicalColumn.IsAssignable = column.IsAssignable;
                    physicalColumn.IsComputed = column.IsComputed;

                    table.Columns.ColumnList.Add(physicalColumn);
                }
                if (dimReference != null)
                {
                    // TODO: This is wrong in general.  We need to ensure that this is always a single column constraint
                    AstTable.AstTableKeyBaseNode primaryKey = dimReference.Dimension.PreferredKey;
                    if (primaryKey == null)
                    {
                        throw new Exception("Error: All Detego Tables need a Primary Key!");
                    }

                    //TODO: We currently support only a single primary key column.  This should be fixed :).
                    PhysicalTSQL.DimensionMapping physicalDimReference = new Ssis2008Emitter.IR.TSQL.DimensionMapping();

                    PhysicalTSQL.Column physicalColumn = new PhysicalTSQL.Column();
                    physicalColumn.Name = dimReference.OutputName;
                    physicalColumn.IsNullable = primaryKey.Columns[0].Column.IsNullable;
                    physicalColumn.Type = Ssis2008Emitter.Emitters.TSQL.PhysicalTypeTranslator.Translate(primaryKey.Columns[0].Column.Type, primaryKey.Columns[0].Column.Length, primaryKey.Columns[0].Column.Precision, primaryKey.Columns[0].Column.Scale, primaryKey.Columns[0].Column.CustomType);
                    physicalColumn.Default = primaryKey.Columns[0].Column.Default;
                    table.Columns.ColumnList.Add(physicalColumn);

                    PhysicalTSQL.ForeignKeyConstraint constraint = new PhysicalTSQL.ForeignKeyConstraint();
                    constraint.Name = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}_{2}", dimReference.Dimension.Name, dimReference.OutputName, primaryKey.Columns[0].Column.Name);
                    constraint.Table = dimReference.Dimension.Name;
                    PhysicalTSQL.Key localKey = new PhysicalTSQL.Key();
                    localKey.Name = physicalColumn.Name;
                    constraint.LocalColumnList.Add(localKey);

                    PhysicalTSQL.Key foreignKey = new PhysicalTSQL.Key();
                    foreignKey.Name = primaryKey.Columns[0].Column.Name;
                    constraint.ForeignColumnList.Add(foreignKey);
                    table.AddForeignKeyConstraint(constraint);

                    // TODO: Do we still need this? table.Columns.DimensionMappingList.Add(physicalDimReference);
                }
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(columnBase, e);
            }
        }

        public static Package Lower(this PhysicalTSQL.Table table)
        {
            Package tablePackage = new Package();
            _CurrentPackage = tablePackage;

            AddConnection(table.ConnectionConfiguration);

            tablePackage.ConstraintMode = "Linear";
            tablePackage.DefaultPlatform = "SSIS2008";
            tablePackage.Log = true;
            tablePackage.LogConnectionName = table.ConnectionConfiguration == null ? null : table.ConnectionConfiguration.Name;
            tablePackage.Name = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", table.Name);

            PhysicalTask.SqlTask createTableTask = new Ssis2008Emitter.IR.Task.SqlTask();
            createTableTask.Name = "Create Table";
            createTableTask.ResultSet = "None";
            createTableTask.Connection = table.ConnectionConfiguration == null ? null : table.ConnectionConfiguration.Name;
            createTableTask.ExecuteDuringDesignTime = false;
            createTableTask.Parent = tablePackage;
            createTableTask.Type = "File";
            createTableTask.Body = new TSQLEmitter.TablePlatformEmitter().Emit(table);
            tablePackage.Tasks.Add(createTableTask);

            return tablePackage;
        }
        #endregion  // Table

        #region Transformation
        public static PhysicalDataFlow.Transformation Lower(this AstTransformation.AstTransformationNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                PhysicalDataFlow.Transformation transformation = new Ssis2008Emitter.IR.DataFlow.Transformation();

                // TODO: Use reflection here to do dynamic lookups and reduce the surface area for new emission
                Type astNodeType = astNode.GetType();
                if (astNodeType.Equals(typeof(AstTransformation.AstAutoNullPatcherNode)))
                {
                    transformation = ((AstTransformation.AstAutoNullPatcherNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstConditionalSplitNode)))
                {
                    transformation = ((AstTransformation.AstConditionalSplitNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstDerivedColumnListNode)))
                {
                    transformation = ((AstTransformation.AstDerivedColumnListNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstDestinationNode)))
                {
                    transformation = ((AstTransformation.AstDestinationNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstIsNullPatcherNode)))
                {
                    transformation = ((AstTransformation.AstIsNullPatcherNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstLookupNode)))
                {
                    transformation = ((AstTransformation.AstLookupNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstOleDbCommandNode)))
                {
                    transformation = ((AstTransformation.AstOleDbCommandNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstSortNode)))
                {
                    transformation = ((AstTransformation.AstSortNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstQuerySourceNode)))
                {
                    transformation = ((AstTransformation.AstQuerySourceNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstXmlSourceNode)))
                {
                    transformation = ((AstTransformation.AstXmlSourceNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstTermLookupNode)))
                {
                    transformation = ((AstTransformation.AstTermLookupNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTransformation.AstUnionAllNode)))
                {
                    transformation = ((AstTransformation.AstUnionAllNode)astNode).Lower();
                }
                else
                {
                    // TODO: Message.Trace(Severity.Error);
                    return null;
                }
                transformation.AstNodeName = (astNode.ReferenceableName == null ? String.Empty : astNode.ReferenceableName);
                transformation.Name = (transformation.Name == null ? String.Empty : transformation.Name) + Guid.NewGuid().ToString();
                transformation.ValidateExternalMetadata = astNode.ValidateExternalMetadata;
                if (astNode.InputPath != null)
                    transformation.InputPath = new InputPath(astNode.InputPath.PathName, astNode.InputPath.Source);
                if (astNode.OutputPath != null)
                    transformation.OutputPath = new OutputPath(astNode.OutputPath.Name);
                return transformation;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        public static PhysicalDataFlow.DerivedColumns Lower(this AstTransformation.AstAutoNullPatcherNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Ssis2008Emitter.IR.DataFlow.DerivedColumns physicalNode = new Ssis2008Emitter.IR.DataFlow.DerivedColumns();
            physicalNode.Name = String.Format(System.Globalization.CultureInfo.InvariantCulture, "AutoPatcher for Fact Table: {0}", astNode.FactTable.ReferenceableName);

            foreach (AstTable.AstTableColumnBaseNode column in astNode.FactTable.Columns)
            {
                if (!column.IsNullable)
                {
                    Ssis2008Emitter.IR.DataFlow.DerivedColumn physicalColumn = new Ssis2008Emitter.IR.DataFlow.DerivedColumn();
                    physicalColumn.Name = column.Name;
                    AstTable.AstTableColumnNode tableColumn = column as AstTable.AstTableColumnNode;
                    // TODO: Use template here?
                    // TODO: Can we assume that dim default is 0?
                    physicalColumn.Expression = String.Format(System.Globalization.CultureInfo.InvariantCulture, "ISNULL({0}) ? {1} : {0}", column.Name, tableColumn == null ? 0.ToString(System.Globalization.CultureInfo.InvariantCulture) : tableColumn.Default);
                    physicalColumn.ReplaceExisting = true;
                    physicalColumn.Parent = physicalNode;
                    physicalNode.Columns.Add(physicalColumn);
                }
            }
            return physicalNode;
        }

        public static PhysicalDataFlow.ConditionalSplit Lower(this AstTransformation.AstConditionalSplitNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            PhysicalDataFlow.ConditionalSplit physicalNode = new Ssis2008Emitter.IR.DataFlow.ConditionalSplit();
            physicalNode.Name = astNode.Name;

            return physicalNode;
        }

        public static PhysicalDataFlow.DerivedColumns Lower(this AstTransformation.AstDerivedColumnListNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Ssis2008Emitter.IR.DataFlow.DerivedColumns physicalNode = new Ssis2008Emitter.IR.DataFlow.DerivedColumns();
            physicalNode.Name = astNode.Name;

            foreach (AstTransformation.AstDerivedColumnNode column in astNode.Columns)
            {
                Ssis2008Emitter.IR.DataFlow.DerivedColumn physicalColumn = new Ssis2008Emitter.IR.DataFlow.DerivedColumn();
                physicalColumn.Name = column.Name;
                physicalColumn.Type = column.Type.ToString();
                physicalColumn.Length = column.Length;
                physicalColumn.Precision = column.Precision;
                physicalColumn.Scale = column.Scale;
                physicalColumn.Expression = column.Expression;

                physicalColumn.ReplaceExisting = column.ReplaceExisting;
                physicalColumn.Parent = physicalNode;
                physicalNode.Columns.Add(physicalColumn);
            }
            return physicalNode;
        }

        public static PhysicalDataFlow.DerivedColumns Lower(this AstTransformation.AstIsNullPatcherNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Ssis2008Emitter.IR.DataFlow.DerivedColumns physicalNode = new Ssis2008Emitter.IR.DataFlow.DerivedColumns();
            physicalNode.Name = astNode.Name;

            foreach (AstTransformation.AstIsNullPatcherColumnNode column in astNode.Columns)
            {
                Ssis2008Emitter.IR.DataFlow.DerivedColumn physicalColumn = new Ssis2008Emitter.IR.DataFlow.DerivedColumn();
                physicalColumn.Name = column.Name;
                // TODO: Use template here?
                physicalColumn.Expression = String.Format(System.Globalization.CultureInfo.InvariantCulture, "ISNULL({0}) ? {1} : {0}", column.Name, column.DefaultValue);
                physicalColumn.ReplaceExisting = true;
                physicalColumn.Parent = physicalNode;
                physicalNode.Columns.Add(physicalColumn);
            }
            return physicalNode;
        }

        public static PhysicalDataFlow.Lookup Lower(this AstTransformation.AstLookupNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Ssis2008Emitter.IR.DataFlow.Lookup physicalNode = new Ssis2008Emitter.IR.DataFlow.Lookup();

            physicalNode.Name = astNode.Name;
            AddConnection(astNode.Connection);
            physicalNode.Connection = astNode.Connection.Name;
            physicalNode.Query = astNode.Query;

            foreach (AstTransformation.AstLookupInputOutputNode input in astNode.Inputs)
            {
                Ssis2008Emitter.IR.DataFlow.LookupInputJoin physicalInput = new Ssis2008Emitter.IR.DataFlow.LookupInputJoin();
                physicalInput.Name = input.LocalColumnName;
                physicalInput.JoinToReferenceColumn = input.RemoteColumnName;
                physicalInput.Parent = physicalNode;
                physicalNode.InputList.Add(physicalInput);
                physicalNode.InputList.Add(physicalInput);
            }

            foreach (AstTransformation.AstLookupInputOutputNode output in astNode.Outputs)
            {
                Ssis2008Emitter.IR.DataFlow.LookupOutputJoin physicalOutput = new Ssis2008Emitter.IR.DataFlow.LookupOutputJoin();
                physicalOutput.Name = output.LocalColumnName;
                physicalOutput.CopyFromReferenceColumn = output.RemoteColumnName;
                physicalOutput.Parent = physicalNode;
                physicalNode.OutputList.Add(physicalOutput);
            }

            return physicalNode;
        }

        public static PhysicalDataFlow.OLEDBCommand Lower(this AstTransformation.AstOleDbCommandNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Ssis2008Emitter.IR.DataFlow.OLEDBCommand physicalNode = new Ssis2008Emitter.IR.DataFlow.OLEDBCommand();
            AddConnection(astNode.Connection);
            physicalNode.Connection = astNode.Connection.Name;
            physicalNode.Name = astNode.Name;
            physicalNode.Command = astNode.Command;

            foreach (AstTransformation.AstDataFlowColumnMappingNode map in astNode.Maps)
            {
                Ssis2008Emitter.IR.DataFlow.Mapping physicalMapping = new Ssis2008Emitter.IR.DataFlow.Mapping();
                physicalMapping.Source = map.SourceName;
                physicalMapping.Destination = map.DestinationName;
                physicalMapping.Parent = physicalNode;
                physicalNode.Mappings.Add(physicalMapping);
            }

            return physicalNode;
        }

        public static PhysicalDataFlow.Transformation Lower(this AstTransformation.AstSortNode astNode)
        {
            throw new NotImplementedException();
        }

        public static PhysicalDataFlow.TermLookup Lower(this AstTransformation.AstTermLookupNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Ssis2008Emitter.IR.DataFlow.TermLookup physicalNode = new Ssis2008Emitter.IR.DataFlow.TermLookup();
            physicalNode.Name = astNode.Name;
            AddConnection(astNode.Connection);
            physicalNode.Connection = astNode.Connection.Name;
            physicalNode.IsCaseSensitive = astNode.IsCaseSensitive;
            physicalNode.RefTermTable = astNode.RefTermTableName;
            physicalNode.RefTermColumn = astNode.RefTermColumnName;

            foreach (AstTransformation.AstTermLookupColumnNode column in astNode.InputColumns)
            {
                Ssis2008Emitter.IR.DataFlow.InputColumn physicalColumn = new Ssis2008Emitter.IR.DataFlow.InputColumn();
                physicalColumn.Name = column.InputColumnName;
                physicalColumn.UsageType = column.InputColumnUsageType;
                physicalColumn.Parent = physicalNode;

                physicalNode.InputColumnList.Add(physicalColumn);
            }
            return physicalNode;
        }

        public static PhysicalDataFlow.UnionAll Lower(this AstTransformation.AstUnionAllNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Ssis2008Emitter.IR.DataFlow.UnionAll physicalNode = new Ssis2008Emitter.IR.DataFlow.UnionAll();
            physicalNode.Name = astNode.Name;

            return physicalNode;
        }

        public static PhysicalDataFlow.OLEDBSource Lower(this AstTransformation.AstXmlSourceNode astNode)
        {
            throw new NotImplementedException();
        }

        public static PhysicalDataFlow.OLEDBSource Lower(this AstTransformation.AstQuerySourceNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            PhysicalDataFlow.OLEDBSource physicalSource = new Ssis2008Emitter.IR.DataFlow.OLEDBSource();

            physicalSource.Body = astNode.Query;
            AddConnection(astNode.Connection);
            physicalSource.Connection = astNode.Connection.Name;
            physicalSource.Name = astNode.Name;

            if (astNode.EvaluateAsExpression == true)
            {
                physicalSource.AccessMode = Ssis2008Emitter.IR.DataFlow.DataFlowSourceQueryAccessMode.SQLCOMMANDFROMVARIABLE;
            }
            else
            {
                physicalSource.AccessMode = Ssis2008Emitter.IR.DataFlow.DataFlowSourceQueryAccessMode.SQLCOMMAND;
            }

            foreach (AstTransformation.AstQuerySourceParameterNode parameter in astNode.Parameters)
            {
                PhysicalDataFlow.DataFlowSourceQueryParameter physicalParameter = new Ssis2008Emitter.IR.DataFlow.DataFlowSourceQueryParameter();
                physicalParameter.Name = parameter.Name;
                physicalParameter.VariableName = parameter.Variable.Name;
                physicalParameter.Parent = physicalSource;

                physicalSource.ParameterMappings.Add(physicalParameter);
            }
            return physicalSource;
        }

        public static PhysicalDataFlow.Destination Lower(this AstTransformation.AstDestinationNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            PhysicalDataFlow.Destination physicalDestination = new Ssis2008Emitter.IR.DataFlow.Destination();
            physicalDestination.AccessMode = astNode.AccessMode.ToString();
            physicalDestination.CheckConstraints = astNode.CheckConstraints;
            AddConnection(astNode.Connection);
            physicalDestination.Connection = astNode.Connection.Name;
            physicalDestination.KeepIdentity = astNode.KeepIdentity;
            physicalDestination.KeepNulls = astNode.KeepNulls;
            physicalDestination.MaximumInsertCommitSize = astNode.MaximumInsertCommitSize;
            physicalDestination.Name = astNode.Name;
            physicalDestination.RowsPerBatch = astNode.RowsPerBatch;
            physicalDestination.Table = astNode.TableName;
            physicalDestination.TableLock = astNode.TableLock;
            physicalDestination.UseStaging = astNode.UseStaging;
            foreach (AstTransformation.AstDataFlowColumnMappingNode map in astNode.Maps)
            {
                PhysicalDataFlow.Mapping physicalmapping = new Ssis2008Emitter.IR.DataFlow.Mapping();
                physicalmapping.Source = map.SourceName;
                physicalmapping.Destination = map.DestinationName;

                physicalDestination.Mappings.Add(physicalmapping);
            }
            return physicalDestination;
        }
        #endregion  // Transformation

        #region Task
        public static PhysicalTask.SqlTask Lower(this AstTask.AstStoredProcNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                PhysicalTSQL.StoredProc physicalStoredProc = new Ssis2008Emitter.IR.TSQL.StoredProc();
                physicalStoredProc.Body = astNode.Body;
                physicalStoredProc.ExecuteDuringDesignTime = astNode.ExecuteDuringDesignTime;
                physicalStoredProc.Name = astNode.Name;
                physicalStoredProc.Columns.Parent = physicalStoredProc;

                foreach (AstTask.AstStoredProcColumnNode column in astNode.Columns)
                {
                    PhysicalTSQL.StoredProcColumn physicalColumn = new Ssis2008Emitter.IR.TSQL.StoredProcColumn();
                    physicalColumn.Default = column.Default;
                    physicalColumn.IsOutput = column.IsOutput;
                    physicalColumn.Name = column.Name;
                    physicalColumn.Parent = physicalStoredProc;
                    physicalColumn.Type = Ssis2008Emitter.Emitters.TSQL.PhysicalTypeTranslator.Translate(column.Type, column.Length, column.Precision, column.Scale, column.CustomType);
                    physicalStoredProc.Columns.ColumnList.Add(physicalColumn);
                }

                TSQLEmitter.StoredProcPlatformEmitter storedProcEmitter = new TSQLEmitter.StoredProcPlatformEmitter();
                string storedProcBody = storedProcEmitter.Emit(physicalStoredProc);

                PhysicalTask.SqlTask sqlTask = new Ssis2008Emitter.IR.Task.SqlTask();
                sqlTask.Body = storedProcBody;
                AddConnection(astNode.Connection);
                sqlTask.Connection = astNode.Connection.Name;
                sqlTask.ExecuteDuringDesignTime = astNode.ExecuteDuringDesignTime;
                sqlTask.Name = astNode.Name;
                sqlTask.ResultSet = "None";
                sqlTask.Type = "File";

                return sqlTask;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        public static Variable Lower(this AstTask.AstVariableNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            Variable variable = new Variable();
            variable.Name = astNode.Name;
            variable.Type = astNode.Type.ToString();
            variable.Value = astNode.Value;

            return variable;
        }

        public static PhysicalTask.Task Lower(this AstTask.AstTaskNode astNode)
        {
            // TODO: Use reflection here to do dynamic lookups and reduce the surface area for new emission
            Type astNodeType = astNode.GetType();
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                if (astNodeType.Equals(typeof(AstTask.AstContainerTaskNode)))
                {
                    return ((AstTask.AstContainerTaskNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTask.AstExecutePackageTaskNode)))
                {
                    return ((AstTask.AstExecutePackageTaskNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTask.AstExecuteSQLTaskNode)))
                {
                    return ((AstTask.AstExecuteSQLTaskNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTask.AstETLRootNode)))
                {
                    return ((AstTask.AstETLRootNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTask.AstStoredProcNode)))
                {
                    return ((AstTask.AstStoredProcNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTask.AstStagingContainerTaskNode)))
                {
                    return ((AstTask.AstStagingContainerTaskNode)astNode).Lower();
                }
                else if (astNodeType.Equals(typeof(AstTask.AstMergeTaskNode)))
                {
                    return ((AstTask.AstMergeTaskNode)astNode).Lower();
                }
                else
                {
                    // TODO: Message.Trace(Severity.Error);
                    return null;
                }
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        public static PhysicalTask.SqlTask Lower(this AstTask.AstMergeTaskNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                PhysicalTask.SqlTask mergeSqlTask = new PhysicalTask.SqlTask();
                Ssis2008Emitter.Emitters.TSQL.PlatformEmitter.MergeEmitter me = new Ssis2008Emitter.Emitters.TSQL.PlatformEmitter.MergeEmitter();

                mergeSqlTask.Name = astNode.Name;
                mergeSqlTask.Type = "File";
                mergeSqlTask.ResultSet = "NONE";
                mergeSqlTask.ExecuteDuringDesignTime = false;
                mergeSqlTask.Connection = astNode.Connection.Name;
                AddConnection(astNode.Connection);
                mergeSqlTask.Body = me.Emit(astNode);

                return mergeSqlTask;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        public static PhysicalTask.SequenceTask Lower(this AstTask.AstStagingContainerTaskNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                TSQLEmitter.TemplatePlatformEmitter createStagingTemplate =
                    new Ssis2008Emitter.Emitters.TSQL.PlatformEmitter.TemplatePlatformEmitter("CreateStagingTable", astNode.Table.Name, astNode.CreateAs);

                IR.TSQL.Table table = astNode.Table.Lower();
                table.Indexes.IndexList.Clear();
                //table.ConstraintList.Clear();
                if (astNode.UseStaticSource == false)
                {
                    table.DefaultValues.DefaultValueList.Clear();
                }
                foreach (string s in astNode.DropConstraints)
                {
                    for (int i = 0; i < table.ConstraintList.Count; i++)
                    {
                        if (String.Compare(s.Trim(), table.ConstraintList[i].Name.Trim(), true) == 0)
                        {
                            table.ConstraintList.RemoveAt(i);
                            break;
                        }
                    }
                }

                foreach (IR.TSQL.Constraint constraint in table.ConstraintList)
                {
                    constraint.Name = String.Format("{0}_{1}", astNode.CreateAs, constraint.Name);
                }
                table.ForeignKeyConstraintList.Clear();
                table.Name = astNode.CreateAs;
                table.ConnectionConfiguration = astNode.StagingConnection.Lower();


                AstTask.AstExecuteSQLTaskNode createStagingTask = new VulcanEngine.IR.Ast.Task.AstExecuteSQLTaskNode();

                createStagingTask.ParentASTNode = astNode;
                createStagingTask.Connection = astNode.StagingConnection;
                AddConnection(astNode.StagingConnection);
                createStagingTask.ExecuteDuringDesignTime = true;
                createStagingTask.Name = astNode.Name;
                //createStagingTask.Type = VulcanEngine.IR.Ast.Task.ExecuteSQLTaskType.Expression;
                createStagingTask.Type = VulcanEngine.IR.Ast.Task.ExecuteSQLTaskType.File;
                //createStagingTask.Body = String.Format("\"{0}\"", createStagingTemplate.Emit(null));
                createStagingTask.Body = new TSQLEmitter.TablePlatformEmitter().Emit(table);

                TSQLEmitter.TemplatePlatformEmitter dropStagingTemplate =
                    new Ssis2008Emitter.Emitters.TSQL.PlatformEmitter.TemplatePlatformEmitter("DropStagingTable", astNode.CreateAs);

                AstTask.AstExecuteSQLTaskNode truncateStagingTask = new VulcanEngine.IR.Ast.Task.AstExecuteSQLTaskNode();

                truncateStagingTask.ParentASTNode = astNode;
                truncateStagingTask.Connection = astNode.StagingConnection;
                truncateStagingTask.ExecuteDuringDesignTime = false;
                truncateStagingTask.Name = astNode.Name;
                truncateStagingTask.Type = VulcanEngine.IR.Ast.Task.ExecuteSQLTaskType.Expression;
                truncateStagingTask.Body = String.Format("\"{0}\"", dropStagingTemplate.Emit(null));

                astNode.Tasks.Insert(0, createStagingTask);
                astNode.Tasks.Add(truncateStagingTask);
                return ((AstTask.AstContainerTaskNode)astNode).Lower();
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        public static PhysicalTask.SequenceTask Lower(this AstTask.AstContainerTaskNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                PhysicalTask.SequenceTask sequenceTask = new PhysicalTask.SequenceTask();
                switch (astNode.TransactionMode)
                {
                    case VulcanEngine.IR.Ast.Task.ContainerTransactionMode.StartOrJoin:
                        sequenceTask.TransactionMode = "Required";
                        break;
                    case VulcanEngine.IR.Ast.Task.ContainerTransactionMode.Join:
                        sequenceTask.TransactionMode = "Supported";
                        break;
                    case VulcanEngine.IR.Ast.Task.ContainerTransactionMode.NoTransaction:
                        sequenceTask.TransactionMode = "NotSupported";
                        break;
                    default:
                        sequenceTask.TransactionMode = "Supported";
                        break;
                }
                sequenceTask.ConstraintMode = astNode.ConstraintMode.ToString();
                sequenceTask.Log = astNode.Log;
                sequenceTask.Name = astNode.Name;

                foreach (AstTask.AstVariableNode variableNode in astNode.Variables)
                {
                    Variable physicalVariableNode = variableNode.Lower();
                    physicalVariableNode.Parent = sequenceTask;
                    sequenceTask.VariableList.Add(physicalVariableNode);
                }

                foreach (AstTask.AstTaskNode taskNode in astNode.Tasks)
                {
                    PhysicalTask.Task physicalTaskNode = taskNode.Lower();
                    if (physicalTaskNode != null)
                    {
                        physicalTaskNode.Parent = sequenceTask;
                        sequenceTask.Tasks.Add(physicalTaskNode);
                    }
                }

                ProcessHelperTables(astNode, sequenceTask);

                if (astNode.Log)
                {
                    PhysicalTask.SequenceTask logSequenceTask = new PhysicalTask.SequenceTask();
                    logSequenceTask.ConstraintMode = AstTask.ContainerConstraintMode.Linear.ToString();
                    logSequenceTask.Log = astNode.Log;
                    logSequenceTask.Name = String.Format("__LOGWRAPPER_{0}", astNode.Name);

                    AddConnection(astNode.LogConnection);
                    logSequenceTask.LogConnectionName = astNode.LogConnection == null ? null : astNode.LogConnection.Name;

                    logSequenceTask.Tasks.Add(CreateLogStartTask(logSequenceTask, astNode.ReferenceableName));
                    logSequenceTask.Tasks.Add(CreateLogReadPreviousValuesTask(logSequenceTask, sequenceTask, astNode.ReferenceableName));

                    logSequenceTask.Tasks.Add(sequenceTask);
                    logSequenceTask.Tasks.Add(CreateLogEndTask(logSequenceTask, sequenceTask, astNode.ReferenceableName));

                    return logSequenceTask;

                }

                return sequenceTask;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        public static PhysicalTask.ExecutePackageTask Lower(this AstTask.AstExecutePackageTaskNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            PhysicalTask.ExecutePackageTask executePackageTask = new Ssis2008Emitter.IR.Task.ExecutePackageTask();
            executePackageTask.Name = astNode.Name;
            executePackageTask.RelativePath = astNode.RelativePath;

            return executePackageTask;
        }

        public static PhysicalTask.SqlTask Lower(this AstTask.AstExecuteSQLTaskNode astNode)
        {
            if (astNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                PhysicalTask.SqlTask sqlTask = new Ssis2008Emitter.IR.Task.SqlTask();
                sqlTask.Body = astNode.Body;
                AddConnection(astNode.Connection);
                sqlTask.Connection = astNode.Connection.Name;
                sqlTask.ExecuteDuringDesignTime = astNode.ExecuteDuringDesignTime;
                sqlTask.Name = astNode.Name;
                sqlTask.ResultSet = astNode.ResultSet.ToString();
                sqlTask.Type = astNode.Type.ToString();
                sqlTask.IsolationLevel = astNode.IsolationLevel;

                foreach (AstTask.AstParameterMappingTypeNode mapping in astNode.Results)
                {
                    PhysicalTask.ExecuteSQLResult result = new Ssis2008Emitter.IR.Task.ExecuteSQLResult();
                    result.Name = mapping.ParameterName;
                    result.VariableName = mapping.Variable.Name;
                    result.Parent = sqlTask;

                    sqlTask.ResultList.Add(result);
                }
                return sqlTask;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(astNode, e);
            }
        }

        public static PhysicalTask.PipelineTask Lower(this AstTask.AstETLRootNode rootNode)
        {
            if (rootNode.AsClassOnly)
            {
                return null;
            }

            try
            {
                PhysicalTask.PipelineTask pipelineTask = new Ssis2008Emitter.IR.Task.PipelineTask();
                pipelineTask.Name = rootNode.Name;
                pipelineTask.DelayValidation = rootNode.DelayValidation;
                pipelineTask.IsolationLevel = rootNode.IsolationLevel;

                ProcessTransformList(rootNode.Transformations, pipelineTask, null, new Dictionary<string, PhysicalDataFlow.Transformation>());
                return pipelineTask;
            }
            catch (Exception e)
            {
                throw new SSISEmitterException(rootNode, e);
            }
        }

        private static PhysicalTask.SequenceTask ProcessHelperTables(AstTask.AstContainerTaskNode astNode, PhysicalTask.SequenceTask sequenceTask)
        {
            foreach (AstTable.AstTableNode helperTableNode in astNode.HelperTables)
            {
                IR.TSQL.Table helperTable = helperTableNode.Lower();
                if (helperTable != null)
                {
                    AstTask.AstExecuteSQLTaskNode createHelperTableTask = new VulcanEngine.IR.Ast.Task.AstExecuteSQLTaskNode();
                    createHelperTableTask.ParentASTNode = astNode;
                    createHelperTableTask.Name = "__Create HelperTable " + helperTable.Name;
                    createHelperTableTask.Connection = helperTableNode.Connection;
                    createHelperTableTask.ExecuteDuringDesignTime = helperTableNode.ExecuteDuringDesignTime;
                    createHelperTableTask.Type = VulcanEngine.IR.Ast.Task.ExecuteSQLTaskType.File;
                    createHelperTableTask.Body = new TSQLEmitter.TablePlatformEmitter().Emit(helperTable);
                    sequenceTask.Tasks.Insert(0, createHelperTableTask.Lower());

                    AstTask.AstExecuteSQLTaskNode dropHelperTableTask = new VulcanEngine.IR.Ast.Task.AstExecuteSQLTaskNode();
                    dropHelperTableTask.ParentASTNode = astNode;
                    dropHelperTableTask.Name = "__Drop HelperTable " + helperTable.Name;
                    dropHelperTableTask.Connection = helperTableNode.Connection;
                    dropHelperTableTask.ExecuteDuringDesignTime = false;
                    dropHelperTableTask.Type = VulcanEngine.IR.Ast.Task.ExecuteSQLTaskType.Expression;
                    dropHelperTableTask.Body = String.Format("\"{0}\"", new TSQLEmitter.TemplatePlatformEmitter("DropHelperTable", helperTable.Name).Emit(helperTable));
                    sequenceTask.Tasks.Add(dropHelperTableTask.Lower());
                }
            }

            return sequenceTask;
        }

        private static PhysicalDataFlow.Transformation ProcessTransformList(VulcanCollection<AstTransformation.AstTransformationNode> transformations, PhysicalTask.PipelineTask pipelineTask, PhysicalDataFlow.Transformation previousNode, Dictionary<string, PhysicalDataFlow.Transformation> componentLibrary)
        {
            PhysicalDataFlow.Transformation firstNode = null;

            List<AstTransformation.AstTransformationNode> allTransformations = new List<VulcanEngine.IR.Ast.Transformation.AstTransformationNode>();

            foreach (AstTransformation.AstTransformationNode astTransformation in transformations)
            {
                if (astTransformation.AsClassOnly)
                {
                    continue;
                }

                if (astTransformation is AstTransformation.AstDataFlowNode)
                {
                    foreach (AstTransformation.AstTransformationNode transformation in ((AstTransformation.AstDataFlowNode)astTransformation).Transformations)
                    {
                        allTransformations.Add(transformation);
                    }
                }
                else
                {
                    allTransformations.Add(astTransformation);
                }
            }

            foreach (AstTransformation.AstTransformationNode astTransformation in allTransformations)
            {
                if (astTransformation.AsClassOnly)
                {
                    continue;
                }

                try
                {
                    PhysicalDataFlow.Transformation physicalTransformation = astTransformation.Lower();
                    physicalTransformation.Parent = pipelineTask;
                    pipelineTask.Transformations.Add(physicalTransformation);

                    // TODO: Fix logical node remapping
                    switch (astTransformation.GetType().Name)
                    {
                        case "AstUnionAllNode":
                            AstTransformation.AstUnionAllNode unionNode = astTransformation as AstTransformation.AstUnionAllNode;
                            Ssis2008Emitter.IR.DataFlow.UnionAll physicalUnionNode = physicalTransformation as Ssis2008Emitter.IR.DataFlow.UnionAll;

                            foreach (AstTransformation.AstUnionAllInputPathNode unionInputPath in unionNode.InputPathCollection)
                            {
                                Ssis2008Emitter.IR.DataFlow.UnionInputPath inputPath = new Ssis2008Emitter.IR.DataFlow.UnionInputPath();

                                inputPath.Name = unionInputPath.PathName;
                                string componentFullName = astTransformation.ParentASTNode.ReferenceableName + VulcanEngine.Common.IRUtility.NamespaceSeparator + unionInputPath.Source;
                                if (componentLibrary.ContainsKey(componentFullName))
                                {
                                    inputPath.Source = componentLibrary[componentFullName].Name;
                                }
                                inputPath.Parent = physicalUnionNode;

                                var physicalMappings = from map in unionInputPath.Maps
                                                       select new Ssis2008Emitter.IR.DataFlow.UnionMapping()
                                                       {
                                                           Input = map.SourceName,
                                                           Output = map.DestinationName
                                                       };
                                inputPath.MappingList = physicalMappings.ToList<Ssis2008Emitter.IR.DataFlow.UnionMapping>();
                                physicalUnionNode.InputPathList.Add(inputPath);
                            }
                            break;
                        case "AstConditionalSplitNode":
                            AstTransformation.AstConditionalSplitNode splitNode = astTransformation as AstTransformation.AstConditionalSplitNode;
                            Ssis2008Emitter.IR.DataFlow.ConditionalSplit physicalSplitNode = physicalTransformation as Ssis2008Emitter.IR.DataFlow.ConditionalSplit;

                            PhysicalDataFlow.Condition defaultCondition = new Ssis2008Emitter.IR.DataFlow.Condition();
                            defaultCondition.Expression = "false";
                            defaultCondition.OutputPath = splitNode.OutputPath.Name;
                            physicalSplitNode.DefaultCondition = defaultCondition;

                            foreach (AstTransformation.AstConditionalSplitOutputNode outputNode in splitNode.Outputs)
                            {
                                PhysicalDataFlow.Condition physicalCondition = new Ssis2008Emitter.IR.DataFlow.Condition();
                                physicalCondition.Expression = outputNode.Expression;
                                physicalCondition.OutputPath = outputNode.Name;
                                physicalSplitNode.ConditionList.Add(physicalCondition);
                            }
                            break;
                    }

                    if (astTransformation.InputPath != null)
                    {
                        string componentFullName = astTransformation.ParentASTNode.ReferenceableName + VulcanEngine.Common.IRUtility.NamespaceSeparator + astTransformation.InputPath.Source;
                        if (componentLibrary.ContainsKey(componentFullName))
                        {
                            physicalTransformation.InputPath.Source = componentLibrary[componentFullName].Name;
                        }
                    }

                    if (firstNode == null)
                    {
                        firstNode = physicalTransformation;
                    }
                    previousNode = physicalTransformation;
                    componentLibrary[physicalTransformation.AstNodeName] = physicalTransformation;
                }
                catch (Exception e)
                {
                    throw new SSISEmitterException(astTransformation, e);
                }
            }
            return firstNode;
        }
        #endregion  // Task

        #region  Auto Logger Code  // Move this to Lowering Phase
        private static Variable AddVariable(PhysicalTask.SequenceTask LogContainer, string name, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections direction, PhysicalTask.SqlTask sqlTask, string type)
        {
            Variable variable = new Variable();
            variable.Name = LogContainer.Name + Resources.Seperator + name;
            variable.Type = type;
            variable.Value = "-1";
            AddVariable(variable, direction, sqlTask);
            return variable;
        }

        private static void AddVariable(Variable variable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections direction, PhysicalTask.SqlTask sqlTask)
        {
            PhysicalTask.ExecuteSQLParameter parameter = new PhysicalTask.ExecuteSQLParameter();
            parameter.Variable = variable;
            parameter.Direction = direction;
            parameter.OleDbType = GetVariableType(variable.Type);
            sqlTask.ParameterList.Add(parameter);
        }

        private static System.Data.OleDb.OleDbType GetVariableType(string variableType)
        {
            System.Data.OleDb.OleDbType retType = System.Data.OleDb.OleDbType.Empty;
            switch (variableType.ToUpper())
            {
                case "STRING": retType = System.Data.OleDb.OleDbType.WChar; break;
                case "INT32": retType = System.Data.OleDb.OleDbType.Integer; break;
                case "INT64": retType = System.Data.OleDb.OleDbType.BigInt; break;
                case "BOOLEAN": retType = System.Data.OleDb.OleDbType.Boolean; break;
                case "Byte": retType = System.Data.OleDb.OleDbType.Binary; break;
                case "Char": retType = System.Data.OleDb.OleDbType.Char; break;
                case "DateTime": retType = System.Data.OleDb.OleDbType.DBTimeStamp; break;
                case "DBNull": retType = System.Data.OleDb.OleDbType.Empty; break;
                case "Double": retType = System.Data.OleDb.OleDbType.Double; break;
                case "Int16": retType = System.Data.OleDb.OleDbType.SmallInt; break;
                case "SByte": retType = System.Data.OleDb.OleDbType.Binary; break;
                case "Single": retType = System.Data.OleDb.OleDbType.Single; break;
                case "UInt32": retType = System.Data.OleDb.OleDbType.UnsignedInt; break;
                case "UInt64": retType = System.Data.OleDb.OleDbType.UnsignedBigInt; break;
            }
            return retType;
        }

        private static string SetVariablePath(string path)
        {
            StringBuilder sPath = new StringBuilder(path.Replace('.', '/'));
            sPath.Insert(0, "/Root/");
            return sPath.ToString();
        }

        public static string GetVariablePathXML(string path)
        {
            string[] pathNodes = path.Split(new char[] { '.' });

            StringBuilder sPath = new StringBuilder();

            for (int i = pathNodes.Length - 1; i >= 0; i--)
            {
                sPath.Insert(0, ">");
                sPath.Insert(0, pathNodes[i]);
                sPath.Insert(0, "<");
                sPath.AppendFormat("</{0}>", pathNodes[i]);
            }

            sPath.Insert(0, "'<Root>");
            sPath.Append("</Root>'");
            return sPath.ToString();
        }

        public static PhysicalTask.SqlTask CreateLogStartTask(PhysicalTask.SequenceTask LogContainer, string path)
        {
            string logTaskName = Resources.Log + Resources.Seperator + _CurrentPackage.Name + Resources.Seperator + LogContainer.Name + Resources.Seperator;

            PhysicalTask.SqlTask logStartTask = new PhysicalTask.SqlTask();

            Variable varLogID = AddVariable(LogContainer, Resources.LogID, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logStartTask, "INT32");
            AddVariable(LogContainer, Resources.LastSuccessfulRunLogID, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logStartTask, "INT32");
            AddVariable(LogContainer, Resources.IsAnotherInstanceCurrentlyRunningLogID, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logStartTask, "INT32");
            logStartTask.Type = "Expression";
            logStartTask.Name = logTaskName + Resources.Start;
            logStartTask.ResultSet = "NONE";
            logStartTask.Connection = LogContainer.LogConnectionName;
            logStartTask.ExecuteDuringDesignTime = false;
            logStartTask.Parent = LogContainer;
            logStartTask.Body = new TSQLEmitter.LogPlatformEmitter().LogStart(LogContainer, _CurrentPackage.Name, LogContainer.Name, GetVariablePathXML(path));

            return logStartTask;
        }

        private static PhysicalTask.SqlTask CreateLogReadPreviousValuesTask(PhysicalTask.SequenceTask logContainer, PhysicalTask.SequenceTask container, string path)
        {
            string logTaskName = Resources.Log + Resources.Seperator + Resources.LoadInto + Resources.Seperator + logContainer.Name;

            PhysicalTask.SqlTask logReadValuesTask = new PhysicalTask.SqlTask();
            logReadValuesTask.Type = "Expression";
            logReadValuesTask.Name = String.Format("LogReadValues-{0}", logContainer.Name);
            logReadValuesTask.Connection = logContainer.LogConnectionName;
            logReadValuesTask.ResultSet = "NONE";

            AddVariable(logContainer, Resources.LastSuccessfulRunLogID, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Input, logReadValuesTask, "INT32");
            AddVariable(logContainer, Resources.StartTime, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logReadValuesTask, "INT32");
            AddVariable(logContainer, Resources.EndTime, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logReadValuesTask, "INT32");
            AddVariable(logContainer, Resources.Status, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logReadValuesTask, "STRING");
            AddVariable(logContainer, Resources.Notes, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logReadValuesTask, "STRING");

            StringBuilder sGetValues = new StringBuilder();
            sGetValues.Append(new TSQLEmitter.LogPlatformEmitter().LogGetPredefinedValues(logContainer));

            foreach (Variable v in container.VariableList)
            {
                sGetValues.Append(" + \n");

                sGetValues.Append(new TSQLEmitter.LogPlatformEmitter().LogGetValue(logContainer, Resources.LastSuccessfulRunLogID, SetVariablePath(path), v.Name));

                AddVariable(v, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, logReadValuesTask);
            }
            logReadValuesTask.Body = sGetValues.ToString();
            return logReadValuesTask;
        }

        public static PhysicalTask.SqlTask CreateLogEndTask(PhysicalTask.SequenceTask logContainer, PhysicalTask.SequenceTask container, string path)
        {
            StringBuilder sLogEndTask = new StringBuilder();

            sLogEndTask.Append(new TSQLEmitter.LogPlatformEmitter().LogPrepareToSetValue(logContainer));
            sLogEndTask.Append(" + \n");

            string logTaskName = Resources.Log + Resources.Seperator + _CurrentPackage.Name + Resources.Seperator + logContainer.Name + Resources.Seperator;
            PhysicalTask.SqlTask logEndTask = new PhysicalTask.SqlTask();//logEndTask.logContainer, context, logTaskName + Resources.Stop);
            logEndTask.Type = "Expression";
            logEndTask.Connection = logContainer.LogConnectionName;
            logEndTask.ResultSet = "NONE";

            Variable varLogID = AddVariable(logContainer, Resources.LogID, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Input, logEndTask, "INT32");

            foreach (Variable variable in container.VariableList)
            {
                sLogEndTask.Append(new TSQLEmitter.LogPlatformEmitter().LogSetValue(logContainer, varLogID.Name, SetVariablePath(path), variable.Name, variable.Name));
                sLogEndTask.Append(" + \n");

                AddVariable(variable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Input, logEndTask);
            }

            sLogEndTask.Append(new TSQLEmitter.LogPlatformEmitter().LogEnd(logContainer, varLogID.Name));

            AddVariable(logContainer, Resources.LogID, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Input, logEndTask, "INT32");

            logEndTask.Body = sLogEndTask.ToString();
            return logEndTask;
        }
        #endregion  // Auto Logger Code

    }
}
