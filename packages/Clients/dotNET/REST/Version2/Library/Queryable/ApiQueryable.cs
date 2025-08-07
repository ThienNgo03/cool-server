
using System.Collections;
using System.Linq.Expressions;

namespace Library.Queryable;

public class ApiQueryable<T> : IOrderedQueryable<T>
{
    #region [ Fields ]

    private QueryStyle? queryStyle;
    #endregion

    #region [ Properties ]

    public ApiQueryable<T> UseQueryStyle(QueryStyle style)
        => new ApiQueryable<T>(Provider, Expression, style);

    internal QueryStyle GetQueryStyle() => this.queryStyle ?? QueryStyle.Rest;
    #endregion

    #region [ CTors ]

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
        this.queryStyle = queryStyle;
    }
    #endregion

    #region [ IOrderedQueryable ]

    public Type ElementType => typeof(T);
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator()
    {
        var result = Provider.Execute<IEnumerable<T>>(Expression);
        return result.GetEnumerator();  
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

}
