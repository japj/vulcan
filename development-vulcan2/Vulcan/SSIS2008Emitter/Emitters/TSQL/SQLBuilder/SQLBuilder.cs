using System;
using System.Collections.Generic;
using System.Text;

namespace Ssis2008Emitter.Emitters.TSQL.SQLBuilder
{
    internal abstract class SQLBuilder
    {
        public const string SEPARATOR = ",\n";
        public const string SEPARATOR_AND = " AND ";
        public const string SEPARATOR_COMMA = ", ";
        public const string NEWLINE = "\n";

        protected StringBuilder _stringBuilder;
        protected bool _bAppendSeparator;
        private string _separator;

        public override string ToString()
        {
            if (_bAppendSeparator)
            {
                return _stringBuilder + SEPARATOR;
            }

            return _stringBuilder.ToString();
        }

        public SQLBuilder(bool bAppendSeparator)
        {
            _bAppendSeparator = bAppendSeparator;        
        }

        protected virtual void CheckAndAppendSeparator()
        {
            if (_stringBuilder.Length > 0)
            {
                AppendSeparator();
            }
        }

        protected virtual void AppendSeparator()
        {
            _stringBuilder.Append(_separator);
        }

        protected virtual void AppendNewLine()
        {
            _stringBuilder.Append(NEWLINE);
        }

        protected virtual void Clear()
        {
            Clear(SEPARATOR);
        }

        protected virtual void Clear(string separator)
        {
            _stringBuilder = new StringBuilder();
            _separator = separator;
        }
    }
}
