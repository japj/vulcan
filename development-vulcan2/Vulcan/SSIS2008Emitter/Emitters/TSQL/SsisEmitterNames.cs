using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.Properties;

namespace Ssis2008Emitter.Emitters.TSQL
{
    internal class SsisEmitterNames
    {
        public static string GetInsertProcedureName(string tableName)
        {
            return Resources.SPPrefix + Resources.Seperator + Resources.Insert + tableName;
        }

        public static string GetInsertAndUpdateProcedureName(string tableName)
        {
            return Resources.SPPrefix + Resources.Seperator + Resources.CheckInsertUpdate + tableName;
        }

        public static string GetCheckAndInsertProcedureName(string tableName)
        {
            return Resources.SPPrefix + Resources.Seperator + Resources.CheckInsert + tableName;
        }
    }
}
