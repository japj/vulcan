using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.Emitters.TSQL
{/*
    internal class SSISStoredProcEmitter : SSISObjectEmitter
    {
        public override Type ObjectType
        {
            get { return typeof(StoredProc); }
        }

        public override SSISEmitterContext Emit(LogicalObject obj, SSISEmitterContext context)
        {
            StoredProc objStoredProc = (StoredProc)obj;
            StoredProcPlatformEmitter storedProcEmitter = new StoredProcPlatformEmitter();
            string sCreateStoredProc = storedProcEmitter.Emit(objStoredProc, context.BaseContext);
            SSISSQLTask sqlTask = SSISSQLTask.CreateInstance(objStoredProc, context, "File", null);
            sqlTask.Emit(sCreateStoredProc, objStoredProc.ExecuteDuringDesignTime);
            return context;
        }
    }*/
}
