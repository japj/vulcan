using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    public abstract class PlatformEmitter
    {
        public static string GetLogicalName(LogicalObject obj)
        {
            return obj == null ? string.Empty : obj.Name;
        }
    }
}
