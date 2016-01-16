namespace AstFramework.Engine.Expressions
{
    public class Token
    {
        public TokenType TokenType { get; private set; }

        public string Text { get; private set; }

        public bool IsDelimiter
        {
            get
            {
                return TokenType == TokenType.ArgumentPrefix
                       || TokenType == TokenType.ArgumentSuffix
                       || TokenType == TokenType.VariablePrefix
                       || TokenType == TokenType.VariableSuffix
                       || TokenType == TokenType.SymbolLookupPrefix
                       || TokenType == TokenType.SymbolLookupSuffix
                       || TokenType == TokenType.FunctionPrefix
                       || TokenType == TokenType.FunctionSuffix;
            }
        }

        public bool IsPrefix
        {
            get
            {
                return TokenType == TokenType.ArgumentPrefix
                       || TokenType == TokenType.VariablePrefix
                       || TokenType == TokenType.SymbolLookupPrefix
                       || TokenType == TokenType.FunctionPrefix;
            }
        }

        public bool IsSuffix
        {
            get
            {
                return TokenType == TokenType.ArgumentSuffix
                       || TokenType == TokenType.VariableSuffix
                       || TokenType == TokenType.SymbolLookupSuffix
                       || TokenType == TokenType.FunctionSuffix;
            }
        }

        public TokenType ClosingTokenType
        {
            get
            {
                switch (TokenType)
                {
                    case TokenType.ArgumentPrefix:
                        return TokenType.ArgumentSuffix;
                    case TokenType.VariablePrefix:
                        return TokenType.VariableSuffix;
                    case TokenType.SymbolLookupPrefix:
                        return TokenType.SymbolLookupSuffix;
                    case TokenType.FunctionPrefix:
                        return TokenType.FunctionSuffix;
                    default:
                        return TokenType.None;
                }
            }
        }

        public Token(string text)
        {
            Text = text;
            TokenType = Constants.GetDelimiterExpressionTokenTypeFromTokenValue(text);
        }
    }
}