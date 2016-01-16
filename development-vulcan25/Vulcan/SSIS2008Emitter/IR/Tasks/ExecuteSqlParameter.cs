using System.Data.OleDb;
using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;

namespace Ssis2008Emitter.IR.Tasks
{
    public class ExecuteSqlParameter
    {
        public string Name { get; private set; }

        public string VariableName { get; set; }

        public int Length { get; set; }

        public ParameterDirections Direction { get; set; }

        public OleDbType OleDBType { get; set; }

        public ExecuteSqlParameter(string name, string variableName, ParameterDirections direction, OleDbType oleDBType, int length)
        {
            Name = name;
            VariableName = variableName;
            Direction = direction;
            OleDBType = oleDBType;
            Length = length;
        }
    }
}
