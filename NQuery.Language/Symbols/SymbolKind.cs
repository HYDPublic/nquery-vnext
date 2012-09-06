namespace NQuery.Language.Symbols
{
    public enum SymbolKind
    {
        BadSymbol,
        BadTable,
        Column,
        SchemaTable,
        DerivedTable,
        TableInstance,
        ColumnInstance,
        CommonTableExpression,
        Variable,
        Parameter,
        Function,
        Aggregate,
        Method,
        Property
    }
}