

using Library.Models;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Library.Queryable;

public class ApiQueryProvider : IQueryProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiQueryProvider(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    public IQueryable CreateQuery(Expression expression)
    {
        var elementType = GetElementType(expression.Type);
        try
        {
            var queryableType = typeof(ApiQueryable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(queryableType, this, expression)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not create query for type {elementType}", ex);
        }
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new ApiQueryable<TElement>(this, expression);
    }

    public object? Execute(Expression expression)
    {
        return Execute<object>(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        Console.WriteLine("🔍 Executing LINQ expression...");

        // Determine query style from the expression
        var queryStyle = GetQueryStyleFromExpression(expression);

        // Create appropriate visitor based on query style
        var visitor = QueryVisitorFactory.Create(queryStyle);

        // Apply expression to visitor (need to implement for IQueryVisitor)
        var queryString = ApplyExpressionToVisitor(visitor, expression);

        // Xây dựng URL đầy đủ
        var requestUrl = string.IsNullOrEmpty(queryString)
            ? _baseUrl
            : $"{_baseUrl}?{queryString}";

        Console.WriteLine($"🌐 Generated URL ({queryStyle}): {requestUrl}");

        try
        {
            // Gọi API HTTP
            var response = _httpClient.GetStringAsync(requestUrl).Result;
            Console.WriteLine($"✅ API Response received: {response.Length} characters");

            // Check if TResult is IEnumerable<T> (typical case for LINQ queries)
            var resultType = typeof(TResult);
            if (resultType.IsGenericType &&
                typeof(IEnumerable<>).IsAssignableFrom(resultType.GetGenericTypeDefinition()))
            {
                // Get the element type (T in IEnumerable<T>)
                var elementType = resultType.GetGenericArguments()[0];

                // Create ApiResponse<T> type
                var apiResponseType = typeof(ApiResponse<>).MakeGenericType(elementType);

                // Deserialize to ApiResponse<T>
                var apiResponse = JsonConvert.DeserializeObject(response, apiResponseType);

                // Get the Data property
                var dataProperty = apiResponseType.GetProperty("Data");
                var data = dataProperty?.GetValue(apiResponse);

                Console.WriteLine($"📦 Extracted data from ApiResponse: {data?.GetType().Name}");

                // Cast to TResult
                return (TResult)data!;
            }
            else
            {
                // Direct deserialization for other types
                var result = JsonConvert.DeserializeObject<TResult>(response);
                Console.WriteLine($"📦 Deserialized to: {typeof(TResult).Name}");
                return result!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error calling API: {ex.Message}");
            throw new InvalidOperationException($"Failed to execute query against API: {ex.Message}", ex);
        }
    }

    private static Type GetElementType(Type seqType)
    {
        var ienum = FindIEnumerable(seqType);
        return ienum?.GetGenericArguments()[0] ?? seqType;
    }

    private static Type? FindIEnumerable(Type seqType)
    {
        if (seqType == null || seqType == typeof(string))
            return null;

        if (seqType.IsArray)
            return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType()!);

        if (seqType.IsGenericType)
        {
            foreach (Type arg in seqType.GetGenericArguments())
            {
                Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                if (ienum.IsAssignableFrom(seqType))
                    return ienum;
            }
        }

        Type[] ifaces = seqType.GetInterfaces();
        if (ifaces.Length > 0)
        {
            foreach (Type iface in ifaces)
            {
                Type? ienum = FindIEnumerable(iface);
                if (ienum != null) return ienum;
            }
        }

        if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            return FindIEnumerable(seqType.BaseType);

        return null;
    }

    /// <summary>
    /// Extracts the query style from the expression tree
    /// </summary>
    private QueryStyle GetQueryStyleFromExpression(Expression expression)
    {
        // Walk the expression tree to find ApiQueryable<T> instances
        var visitor = new QueryStyleExtractor();
        visitor.Visit(expression);
        return visitor.QueryStyle;
    }

    /// <summary>
    /// Applies the expression to the visitor and returns the query string
    /// </summary>
    private string ApplyExpressionToVisitor(IQueryVisitor visitor, Expression expression)
    {
        if (visitor is ExpressionVisitor expressionVisitor)
        {
            expressionVisitor.Visit(expression);
            return visitor.ToQueryString();
        }

        // Fallback for non-ExpressionVisitor implementations
        return string.Empty;
    }
}

/// <summary>
/// Helper visitor to extract QueryStyle from expression tree
/// </summary>
internal class QueryStyleExtractor : ExpressionVisitor
{
    public QueryStyle QueryStyle { get; private set; } = QueryStyle.Rest; // Default to REST

    protected override Expression VisitConstant(ConstantExpression node)
    {
        // Check if the constant is an ApiQueryable of any type
        if (node.Value != null && node.Value.GetType().IsGenericType)
        {
            var genericType = node.Value.GetType().GetGenericTypeDefinition();
            if (genericType == typeof(ApiQueryable<>))
            {
                // Use reflection to get the QueryStyle
                var getQueryStyleMethod = node.Value.GetType().GetMethod("GetQueryStyle",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (getQueryStyleMethod != null)
                {
                    QueryStyle = (QueryStyle)getQueryStyleMethod.Invoke(node.Value, null)!;
                }
            }
        }

        return base.VisitConstant(node);
    }
}