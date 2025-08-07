namespace Library.Queryable;

public static class QueryVisitorFactory
{
    public static IQueryVisitor Create(QueryStyle style)
        => style switch
        {
            QueryStyle.Rest => new ExpressionToRestQueryVisitor(),
            QueryStyle.Odata => new ExpressionToOdataQueryVisitor(),
            _ => new ExpressionToRestQueryVisitor()
        };
}
