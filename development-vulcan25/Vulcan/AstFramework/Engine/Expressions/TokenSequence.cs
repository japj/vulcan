using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using AstFramework.Engine.Binding;
using Microsoft.CSharp;

namespace AstFramework.Engine.Expressions
{
    public class TokenSequence
    {
        public string Input { get; private set; }

        public Collection<Token> Tokens { get; private set; }

        private int _unresolvedTemplateArgumentCount;

        public TokenSequence(string input)
        {
            Input = input;
            Tokens = new Collection<Token>();
            Tokenize();
            Validate();
        }

        public bool RequiresProcessing
        {
            get { return Tokens.Count > 1; }
        }

        public bool RequiresTemplateArguments
        {
            get { return _unresolvedTemplateArgumentCount > 0; }
        }

        public void Validate()
        {
            var tokenStack = new Stack<Token>();
            foreach (var token in Tokens)
            {
                if (token.IsPrefix)
                {
                    tokenStack.Push(token);
                }

                if (token.IsSuffix)
                {
                    var currentStackTop = tokenStack.Pop();
                    while (!currentStackTop.IsPrefix)
                    {
                        currentStackTop = tokenStack.Pop();
                    }

                    if (currentStackTop.ClosingTokenType != token.TokenType)
                    {
                        throw new InvalidOperationException("Unbalanced prefix and suffix tokens.");
                    }
                }
            }

            if (tokenStack.Count != 0)
            {
                throw new InvalidOperationException("Token stack not empty at conclusion of processing.");
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public string Process(SymbolTable symbolTable, Dictionary<string, string> arguments)
        {
            return ProcessSequence(null, Tokens.GetEnumerator(), symbolTable, arguments);
        }

        private void Tokenize()
        {
            string[] tokenStrings = Regex.Split(Input, Constants.DelimiterRegex);

            foreach (var tokenString in tokenStrings)
            {
                var token = new Token(tokenString);
                if (token.TokenType == TokenType.ArgumentPrefix)
                {
                    ++_unresolvedTemplateArgumentCount;
                }

                Tokens.Add(token);
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private string ProcessSequence(Token prefixToken, IEnumerator<Token> tokens, SymbolTable symbolTable, Dictionary<string, string> arguments)
        {
            var builder = new StringBuilder();
            while (tokens.MoveNext())
            {
                Token currentToken = tokens.Current;

                if (currentToken.TokenType == TokenType.StringLiteral)
                {
                    builder.Append(currentToken.Text);
                }
                else if (currentToken.IsPrefix)
                {
                    builder.Append(ProcessSequence(currentToken, tokens, symbolTable, arguments));
                }
                else if (currentToken.IsSuffix)
                {
                    if (currentToken.TokenType == prefixToken.ClosingTokenType)
                    {
                        return PeformReplacement(prefixToken, builder.ToString(), symbolTable, arguments);
                    }

                    throw new InvalidOperationException("Unbalance prefix and suffix tokens.");
                }
            }

            if (prefixToken != null)
            {
                throw new InvalidOperationException("Invalid prefix token.");
            }

            return builder.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "symbolTable", Justification = "Maintaining parameter for future use.")]
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private string PeformReplacement(Token prefixToken, string text, SymbolTable symbolTable, Dictionary<string, string> arguments)
        {
            switch (prefixToken.TokenType)
            {
                case TokenType.ArgumentPrefix:
                    return ReplaceArgument(text, arguments);
                case TokenType.FunctionPrefix:
                    return ReplaceFunction(text);
                case TokenType.SymbolLookupPrefix:
                    throw new NotImplementedException("Symbol Table Lookup replacements are not available in this version of the language.");
                case TokenType.VariablePrefix:
                    throw new NotImplementedException("Variable replacements are not available in this version of the language.");
                default:
                    throw new InvalidOperationException("Unsupported Replacement Token Type.");
            }
        }

        private string ReplaceArgument(string text, Dictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey(text))
            {
                throw new InvalidOperationException("Argument name not found.");
            }

            --_unresolvedTemplateArgumentCount;
            return arguments[text];
        }

        // TODO: Should we run this in an isolated AppDomain
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static string ReplaceFunction(string text)
        {
            string code = String.Format(CultureInfo.InvariantCulture, AssemblyWrapper, text);

            var compilerParams = new CompilerParameters();

            compilerParams.GenerateInMemory = true;
            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.GenerateExecutable = false;
            compilerParams.CompilerOptions = "/optimize";

            string[] references = { "System.dll" };
            compilerParams.ReferencedAssemblies.AddRange(references);

            var provider = new CSharpCodeProvider();
            CompilerResults compile = provider.CompileAssemblyFromSource(compilerParams, code);

            if (compile.Errors.HasErrors)
            {
                var builder = new StringBuilder("Compile error: ");
                foreach (CompilerError ce in compile.Errors)
                {
                    builder.Append("rn" + ce);
                }

                throw new InvalidOperationException(text);
            }

            Module module = compile.CompiledAssembly.GetModules()[0];
            if (module != null)
            {
                Type stubType = module.GetType("Varigence.Isolated.VulcanCompilerRuntime.StubType");
                if (stubType != null)
                {
                    MethodInfo methInfo = stubType.GetMethod("StubMethod");
                    if (methInfo != null)
                    {
                        return (string)methInfo.Invoke(null, null);
                    }
                }
            }

            throw new InvalidOperationException("Unable to execute provided C# code.");
        }

        private const string AssemblyWrapper =
            @"using System;

namespace Varigence.Isolated.VulcanCompilerRuntime
{{
    public static class StubType
    {{
        public static string StubMethod()
        {{
            return ({0});
        }}
    }}
}}
";
    }
}