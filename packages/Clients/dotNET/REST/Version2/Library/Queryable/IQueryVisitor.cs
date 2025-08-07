

namespace Library.Queryable;

public interface IQueryVisitor
{
    string ToQueryString();
}

public enum QueryStyle
{
    Rest,
    Odata
}
