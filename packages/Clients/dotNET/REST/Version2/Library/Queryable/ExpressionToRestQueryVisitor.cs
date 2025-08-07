using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace Library.Queryable;

/// <summary>
/// REST-style query visitor that converts LINQ expressions to REST API query parameters
/// </summary>
public class ExpressionToRestQueryVisitor : ExpressionVisitor, IQueryVisitor
{
    private readonly List<string> _searchTerms = new();
    private readonly List<string> _filters = new();
    private readonly StringBuilder _orderBuilder = new();
    private int _skip = 0;
    private int _take = int.MaxValue;

    public string ToQueryString()
    {
        var queryParams = new List<string>();

        // REST style search
        if (_searchTerms.Count > 0)
        {
            queryParams.Add($"search={HttpUtility.UrlEncode(string.Join(" ", _searchTerms))}");
        }

        // REST style individual filters
        queryParams.AddRange(_filters);

        // REST style ordering
        if (_orderBuilder.Length > 0)
        {
            queryParams.Add($"sort={HttpUtility.UrlEncode(_orderBuilder.ToString())}");
        }

        // REST style pagination
        if (_skip > 0 || _take != int.MaxValue)
        {
            var pageSize = _take != int.MaxValue ? _take : 20;
            var pageIndex = _skip > 0 ? (_skip / pageSize) : 0;

            queryParams.Add($"pageIndex={pageIndex}");
            queryParams.Add($"pageSize={pageSize}");
        }

        return string.Join("&", queryParams);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Handle LINQ methods first
        switch (node.Method.Name)
        {
            case "Where":
                if (node.Arguments.Count >= 2)
                {
                    // Extract lambda from UnaryExpression
                    var whereArgument = node.Arguments[1];
                    LambdaExpression lambda;

                    if (whereArgument is UnaryExpression unary && unary.Operand is LambdaExpression)
                    {
                        lambda = (LambdaExpression)unary.Operand;
                    }
                    else if (whereArgument is LambdaExpression directLambda)
                    {
                        lambda = directLambda;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Unexpected Where argument type: {whereArgument.GetType()}");
                        return Visit(node.Arguments[0]);
                    }

                    // Process the lambda body
                    ProcessWhereExpression(lambda.Body);
                }
                return Visit(node.Arguments[0]); // Source

            case "OrderBy":
            case "OrderByDescending":
                if (node.Arguments.Count >= 2)
                {
                    if (_orderBuilder.Length > 0) _orderBuilder.Append(",");

                    // Extract property name from lambda
                    var lambda = (LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand;
                    var propertyName = GetPropertyName(lambda.Body);

                    _orderBuilder.Append(propertyName);
                    if (node.Method.Name == "OrderByDescending")
                    {
                        _orderBuilder.Append("_desc");
                    }
                    else
                    {
                        _orderBuilder.Append("_asc");
                    }
                }
                return Visit(node.Arguments[0]);

            case "ThenBy":
            case "ThenByDescending":
                if (node.Arguments.Count >= 2)
                {
                    if (_orderBuilder.Length > 0) _orderBuilder.Append(",");

                    var lambda = (LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand;
                    var propertyName = GetPropertyName(lambda.Body);

                    _orderBuilder.Append(propertyName);
                    if (node.Method.Name == "ThenByDescending")
                    {
                        _orderBuilder.Append("_desc");
                    }
                    else
                    {
                        _orderBuilder.Append("_asc");
                    }
                }
                return Visit(node.Arguments[0]);

            case "Skip":
                if (node.Arguments.Count >= 2 && node.Arguments[1] is ConstantExpression skipConst)
                {
                    _skip = (int)skipConst.Value!;
                }
                return Visit(node.Arguments[0]);

            case "Take":
                if (node.Arguments.Count >= 2 && node.Arguments[1] is ConstantExpression takeConst)
                {
                    _take = (int)takeConst.Value!;
                }
                return Visit(node.Arguments[0]);
        }

        // Handle string methods like Contains, StartsWith, EndsWith
        if (node.Method.DeclaringType == typeof(string))
        {
            var propertyName = GetPropertyName(node.Object);
            var value = GetConstantValue(node.Arguments[0]);

            switch (node.Method.Name)
            {
                case "Contains":
                    // For REST, Contains becomes a search term
                    _searchTerms.Add(value.Trim('\''));
                    return node;
                case "StartsWith":
                    _filters.Add($"{propertyName}_startswith={HttpUtility.UrlEncode(value.Trim('\''))}");
                    return node;
                case "EndsWith":
                    _filters.Add($"{propertyName}_endswith={HttpUtility.UrlEncode(value.Trim('\''))}");
                    return node;
            }
        }

        return base.VisitMethodCall(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        return Visit(node.Body);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.NodeType == ExpressionType.AndAlso)
        {
            Visit(node.Left);
            Visit(node.Right);
            return node;
        }
        else if (node.NodeType == ExpressionType.OrElse)
        {
            // REST APIs typically don't handle OR well in query params
            // We'll treat it as AND for simplicity
            Visit(node.Left);
            Visit(node.Right);
            return node;
        }

        // Handle comparison operators
        var left = GetPropertyName(node.Left);
        var right = GetConstantValue(node.Right);

        var paramName = node.NodeType switch
        {
            ExpressionType.Equal => left,
            ExpressionType.NotEqual => $"{left}_ne",
            ExpressionType.GreaterThan => $"{left}_gt",
            ExpressionType.GreaterThanOrEqual => $"{left}_gte",
            ExpressionType.LessThan => $"{left}_lt",
            ExpressionType.LessThanOrEqual => $"{left}_lte",
            _ => left
        };

        _filters.Add($"{paramName}={HttpUtility.UrlEncode(right.Trim('\''))}");
        return node;
    }

    private void ProcessWhereExpression(Expression expression)
    {
        switch (expression)
        {
            case BinaryExpression binary:
                VisitBinary(binary);
                break;

            case MethodCallExpression method when method.Method.Name == "Contains":
                // Handle string.Contains()
                if (method.Object is MemberExpression member && method.Arguments.Count > 0)
                {
                    var value = GetConstantValue(method.Arguments[0]);
                    _searchTerms.Add(value.Trim('\''));
                }
                break;

            default:
                Console.WriteLine($"⚠️ Unsupported Where expression type: {expression.GetType()}");
                break;
        }
    }

    private string GetPropertyName(Expression? expression)
    {
        return expression switch
        {
            MemberExpression member => member.Member.Name.ToLowerInvariant(),
            UnaryExpression unary when unary.Operand is MemberExpression memberExpr =>
                memberExpr.Member.Name.ToLowerInvariant(),
            _ => "unknown"
        };
    }

    private string GetConstantValue(Expression? expression)
    {
        if (expression is ConstantExpression constant)
        {
            if (constant.Value is string str)
                return $"'{str}'";
            if (constant.Value is DateTime dt)
                return $"'{dt:yyyy-MM-ddTHH:mm:ss}'";
            return constant.Value?.ToString() ?? "null";
        }

        if (expression is MemberExpression member && member.Expression is ConstantExpression constExpr)
        {
            var container = constExpr.Value;
            var value = member.Member switch
            {
                System.Reflection.FieldInfo field => field.GetValue(container),
                System.Reflection.PropertyInfo prop => prop.GetValue(container),
                _ => null
            };

            if (value is string str)
                return $"'{str}'";
            if (value is DateTime dt)
                return $"'{dt:yyyy-MM-ddTHH:mm:ss}'";
            return value?.ToString() ?? "null";
        }

        return "null";
    }
}
