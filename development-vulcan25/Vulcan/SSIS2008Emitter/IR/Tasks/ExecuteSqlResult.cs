namespace Ssis2008Emitter.IR.Tasks
{
    public class ExecuteSqlResult
    {
        public string Name { get; set; }

        public string VariableName { get; set; }

        public ExecuteSqlResult(string name, string variableName)
        {
            Name = name;
            VariableName = variableName;
        }
    }
}
