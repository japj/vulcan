namespace DataflowEngine.Statements
{
    public enum Opcode
    {
        AssignNew,
        AssignReplace,
        AssignRemap,
        LoadSetFromSelectQuery,
        RowFromSet,
        IndexIntoSet,
        StoreIntoDestination,
        MergeSetIntoSet,
        Phi,
    }
}
