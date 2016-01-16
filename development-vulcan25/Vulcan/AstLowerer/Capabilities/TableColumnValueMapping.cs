namespace AstLowerer.Capabilities
{
    public class TableColumnValueMapping
    {
        public string ColumnName { get; private set; }

        public string ColumnValue { get; private set; }

        public MappingOperator MappingOperator { get; private set; }

        public string OperatorString
        {
            get
            {
                switch (MappingOperator)
                {
                    case MappingOperator.CompareEqual: return "=";
                    case MappingOperator.CompareNotEqual: return "<>";
                    case MappingOperator.CompareGreaterThan: return ">";
                    case MappingOperator.CompareLessThan: return "<";
                    case MappingOperator.CompareGreaterThanOrEqual: return ">=";
                    case MappingOperator.CompareLessThanOrEqual: return "<=";
                    case MappingOperator.CompareIs: return "IS";
                    case MappingOperator.CompareIsNot: return "IS NOT";
                    case MappingOperator.Assign: return "=";
                    default: return null;
                }
            }
        }

        public TableColumnValueMapping(string columnName, string columnValue) : this(columnName, columnValue, MappingOperator.None)
        {
        }

        public TableColumnValueMapping(string columnName, string columnValue, MappingOperator mappingOperator)
        {
            ColumnName = columnName;
            ColumnValue = columnValue;
            MappingOperator = mappingOperator;
        }
    }

    public enum MappingOperator
    {
        None,
        CompareEqual,
        CompareNotEqual,
        CompareGreaterThan,
        CompareLessThan,
        CompareGreaterThanOrEqual,
        CompareLessThanOrEqual,
        CompareIs,
        CompareIsNot,
        Assign,
    }
}