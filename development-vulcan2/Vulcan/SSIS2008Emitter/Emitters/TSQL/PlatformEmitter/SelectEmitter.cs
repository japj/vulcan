using System;
using System.Collections.Generic;
using System.Text;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    public class SelectEmitter : PlatformEmitter
    {
        public string Emit(string tableName, string columns, string where)
        {
            TemplatePlatformEmitter teSelect = new TemplatePlatformEmitter("SimpleSelect", columns, tableName);
            StringBuilder sSelect = new StringBuilder(teSelect.Emit(null));

            if (!string.IsNullOrEmpty(where))
            {
                TemplatePlatformEmitter teWhere = new TemplatePlatformEmitter("SimpleWhere", where);
                sSelect.Append(teWhere.Emit(null));
            }

            return sSelect.ToString();
        }
    }
}
