using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.TSQL;
using Ssis2008Emitter.Emitters.TSQL.SQLBuilder;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    public class StoredProcPlatformEmitter : PlatformEmitter
    {
        public string Emit(StoredProc storedProc)
        {
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("StoredProc", storedProc.Name, new StoredProcColumnsBuilder(storedProc.Columns).Build(), storedProc.Body);
            return te.Emit(storedProc);
        }
    }
}
