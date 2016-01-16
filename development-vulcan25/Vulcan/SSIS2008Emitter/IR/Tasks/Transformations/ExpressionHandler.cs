using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    public static class ExpressionHandler
    {
        /// <summary>
        /// This method parses a friendly Derived Columns expression entered by the user into a Lineage-ID based
        /// expression which is required by SSIS, using a regular-expression based parser.
        /// Additionally, it will set the Input Column Usage for any columns found in the expression.
        /// </summary>
        /// <param name="expression">Expression to be parsed.</param>
        /// <param name="transformation">Transformation to use for evaluating the lineage IDs</param>
        /// <param name="vi">Transformation Virtual Input used to search for input columns.</param>
        /// <param name="inputColumnUsageType">DTSUsageType for columns mapped in this expression.</param>
        /// <returns>Expression struct with the pre-processed and post-processed expression.</returns>
        public static Expression ExpressionCleanerAndInputMapBuilder(string expression, Transformation transformation, IDTSVirtualInput100 vi, DTSUsageType inputColumnUsageType)
        {
            Expression exp = new Expression();
            exp.OriginalExpression = expression;
            exp.ProcessedExpression = expression;
            exp.FriendlyExpression = expression;
            exp.ContainsId = false;

            foreach (IDTSVirtualInputColumn100 vcol in vi.VirtualInputColumnCollection)
            {
                string regexString = String.Format(CultureInfo.CurrentCulture, "(\"(?:[^\"]|(?<=\\\\)\")*\")|(?<vCol>(?<!@\\[?|:)\\[?\\b{0}\\b\\]?)", Regex.Escape(vcol.Name));
                var regex = new Regex(regexString, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                int groupNumber = regex.GroupNumberFromName("vCol");

                var processedEme = new ExpressionMatchEvaluatorStruct(groupNumber, "#" + vcol.LineageID, transformation, vi, inputColumnUsageType, vcol);
                var friendlyEme = new ExpressionMatchEvaluatorStruct(groupNumber, vcol.Name, null, null, DTSUsageType.UT_IGNORED, null);

                exp.ProcessedExpression = regex.Replace(exp.ProcessedExpression, new MatchEvaluator(processedEme.EvaluateMatch));
                exp.FriendlyExpression = regex.Replace(exp.FriendlyExpression, new MatchEvaluator(friendlyEme.EvaluateMatch));
            }

            if (exp.ProcessedExpression != exp.OriginalExpression)
            {
                exp.ContainsId = true;
            }

            return exp;
        }
    }

    internal struct ExpressionMatchEvaluatorStruct
    {
        private int groupNum;
        private string text;
        private Transformation trans;
        private IDTSVirtualInput100 vi;
        private DTSUsageType inputColumnUsageType;
        private IDTSVirtualInputColumn100 vcol;

        public ExpressionMatchEvaluatorStruct(
            int groupNum,
            string text,
            Transformation trans,
            IDTSVirtualInput100 vi,
            DTSUsageType inputColumnUsageType,
            IDTSVirtualInputColumn100 vcol)
        {
            this.groupNum = groupNum;
            this.text = text;
            this.trans = trans;
            this.vi = vi;
            this.inputColumnUsageType = inputColumnUsageType;
            this.vcol = vcol;
        }

        public string EvaluateMatch(Match m)
        {
            if (m.Groups[groupNum].Success)
            {
                if (trans != null && vi != null && vcol != null)
                {
                    trans.SetInputColumnUsage(0, vcol.Name, vi, vcol, inputColumnUsageType, false);
                }

                return text;
            }

            return m.Value;
        }
    }

    public struct Expression
    {
        public string OriginalExpression { get; set; }

        public string ProcessedExpression { get; set; }

        public string FriendlyExpression { get; set; }

        public bool ContainsId { get; set; }

        public override int GetHashCode()
        {
            return OriginalExpression.GetHashCode() ^ ProcessedExpression.GetHashCode() ^
                   FriendlyExpression.GetHashCode() ^ ContainsId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Expression))
            {
                return false;
            }

            return Equals((Expression)obj);
        }

        public bool Equals(Expression other)
        {
            return OriginalExpression != other.OriginalExpression || ProcessedExpression != other.ProcessedExpression ||
                   FriendlyExpression != other.FriendlyExpression || ContainsId != other.ContainsId;
        }

        public static bool operator ==(Expression expression1, Expression expression2)
        {
            return expression1.Equals(expression2);
        }

        public static bool operator !=(Expression expression1, Expression expression2)
        {
            return !expression1.Equals(expression2);
        }
    }
}
