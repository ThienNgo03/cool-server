
using System.Collections;
using System.Linq.Expressions;

namespace Library.Queryable;

public class ApiQueryable<T> : IOrderedQueryable<T>
{
    private QueryStyle? _queryStyle;

    public ApiQueryable(IQueryProvider provider, Expression expression)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public ApiQueryable(IQueryProvider provider)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Expression = Expression.Constant(this);
    }

    private ApiQueryable(IQueryProvider provider, Expression expression, QueryStyle? queryStyle)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        _queryStyle = queryStyle;
    }

    public Type ElementType => typeof(T);
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    /// <summary>
    /// Sets the query style for this queryable (OData or REST)
    /// </summary>
    /// <param name="style">The query style to use</param>
    /// <returns>A new queryable with the specified query style</returns>
    public ApiQueryable<T> UseQueryStyle(QueryStyle style)
    {
        return new ApiQueryable<T>(Provider, Expression, style);
    }

    /// <summary>
    /// Gets the current query style, defaulting to REST if not set
    /// </summary>
    /// <returns>The query style for this queryable</returns>
    internal QueryStyle GetQueryStyle() => _queryStyle ?? QueryStyle.Rest;

    public IEnumerator<T> GetEnumerator()
    {
        // Khi GetEnumerator được gọi (ví dụ: ToList(), foreach), 
        // chúng ta yêu cầu Provider thực thi truy vấn
        var result = Provider.Execute<IEnumerable<T>>(Expression);
        return result.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
