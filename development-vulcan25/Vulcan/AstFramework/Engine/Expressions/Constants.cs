using System;
using System.Globalization;

namespace AstFramework.Engine.Expressions
{
    public static class Constants
    {
        public const string ArgumentPrefix = "{$";

        public const string ArgumentSuffix = "$}";

        public const string VariablePrefix = "{@";

        public const string VariableSuffix = "@}";

        public const string SymbolLookupPrefix = "{?";

        public const string SymbolLookupSuffix = "?}";

        public const string FunctionPrefix = "{!";

        public const string FunctionSuffix = "!}";

        public const string DelimiterRegex = @"({\$)|(\$})|({@)|(@})|({\?)|(\?})|({!)|(!})";

        // TODO: Can this be done more cleanly with similar performance using a declarative model - i.e. put attributes on the enum values
        public static TokenType GetDelimiterExpressionTokenTypeFromTokenValue(string tokenValue)
        {
            switch (tokenValue)
            {
                case ArgumentPrefix:
                    return TokenType.ArgumentPrefix;
                case ArgumentSuffix:
                    return TokenType.ArgumentSuffix;
                case VariablePrefix:
                    return TokenType.VariablePrefix;
                case VariableSuffix:
                    return TokenType.VariableSuffix;
                case SymbolLookupPrefix:
                    return TokenType.SymbolLookupPrefix;
                case SymbolLookupSuffix:
                    return TokenType.SymbolLookupSuffix;
                case FunctionPrefix:
                    return TokenType.FunctionPrefix;
                case FunctionSuffix:
                    return TokenType.FunctionSuffix;
                default:
                    return TokenType.StringLiteral;
            }
        }
    }
}