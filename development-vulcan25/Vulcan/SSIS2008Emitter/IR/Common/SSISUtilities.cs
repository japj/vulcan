using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Common
{
    internal static class SSISUtilities
    {
        public static string EvaluateSSISExpression(string expression, DTS.DtsContainer container)
        {
            var dtsVariable = 
                container.Variables.Add("__" + Guid.NewGuid().ToString("N"), false, "User", null);
            dtsVariable.EvaluateAsExpression = true;
            dtsVariable.Expression = expression;
            
            string retval = dtsVariable.Value.ToString();
            container.Variables.Remove(dtsVariable.ID);

            return retval;
        }
    }
}
