using System;
using System.Globalization;
using System.Text;
using AstFramework;

namespace VulcanEngine.Common
{
    public sealed class VulcanMessage
    {
        public string FileName { get; private set; }

        public int Line { get; private set; }

        public int Offset { get; private set; }

        public Severity Severity { get; private set; }

        public string Code { get; private set; }

        public Exception InnerException { get; private set; }

        public string Message { get; private set; }

        public string AnnotatedMessage { get; private set; }

        public VulcanMessage(string fileName, int line, int offset, Severity severity, string code, Exception innerException, string message, params object[] formatParameters)
        {
            FileName = fileName;
            Line = line;
            Offset = offset;
            Severity = severity;
            Code = code;
            InnerException = innerException;
            
            if (formatParameters == null || formatParameters.Length == 0)
            {
                message = FormatScrub(message);
            }

            Message = String.Format(CultureInfo.InvariantCulture, message, formatParameters);

            var fileHeaderBuilder = new StringBuilder();
            if (!String.IsNullOrEmpty(FileName))
            {
                fileHeaderBuilder.Append(FileName);
                if (Line > -1)
                {
                    fileHeaderBuilder.Append('(');
                    fileHeaderBuilder.Append(Line);
                    if (Offset > -1)
                    {
                        fileHeaderBuilder.Append(',');
                        fileHeaderBuilder.Append(Offset);
                    }

                    fileHeaderBuilder.Append(')');
                }

                fileHeaderBuilder.Append(": ");
            }

            if (Severity == Severity.Error || Severity == Severity.Warning || Severity == Severity.Debug)
            {
                fileHeaderBuilder.AppendFormat("{0} {1}: ", Severity, Code);
            }

            fileHeaderBuilder.Append(Message);
            AnnotatedMessage = fileHeaderBuilder.ToString();
        }

        private static string FormatScrub(string message)
        {
            return message.Replace("{", "{{").Replace("}", "}}");
        }

        public override string ToString()
        {
            return AnnotatedMessage;
        }
    }
}
