using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;

namespace Ssis2008Emitter.Emitters.TSQL.SQLBuilder
{
    internal class KeysBuilder : SQLBuilder
    {
        private IList<Key> _keys;

        public KeysBuilder(IList<Key> keys) : this(keys, false) { }

        public KeysBuilder(IList<Key> keys, bool bAppendSeparator) : base(bAppendSeparator)
        {
            _keys = keys;
        }

        public string Build(string sPrefix, out string sKeysName)
        {
            Clear();
            StringBuilder Names = new StringBuilder(sPrefix);
            bool bBuildKeyName = String.IsNullOrEmpty(sPrefix) || sPrefix.Length < 4;
            foreach (Key k in _keys)
            {
                CheckAndAppendSeparator();
                Append(k);

                if (bBuildKeyName)
                {
                    Names.Append("_");
                    Names.Append(k.Name);
                }
            }
            sKeysName = Names.ToString();
            return ToString();
        }

        public string BuildForeignKeys()
        {
            Clear();
            foreach (Key k in _keys)
            {
                CheckAndAppendSeparator();
                AppendKeyName(k);
            }
            return ToString();
        }

        public string Build()
        {
            Clear();
            foreach (Key k in _keys)
            {
                CheckAndAppendSeparator();
                Append(k);
            }
            return ToString();
        }

        private void Append(Key k)
        {
            _stringBuilder.AppendFormat("[{0}] {1}", k.Name, k.SortOrder);
        }

        private void AppendKeyName(Key k)
        {
            _stringBuilder.AppendFormat("[{0}] ", k.Name);
        }
    }
}
