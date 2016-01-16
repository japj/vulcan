namespace AstFramework.Engine.Expressions
{
    public enum TokenType
    {
        None,
        ArgumentPrefix, 
        ArgumentSuffix, 
        VariablePrefix, 
        VariableSuffix, 
        SymbolLookupPrefix, 
        SymbolLookupSuffix,
        FunctionPrefix,
        FunctionSuffix,
        StringLiteral
    }
}