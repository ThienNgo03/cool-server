
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace Library.Queryable;

public class ExpressionToOdataQueryVisitor : ExpressionVisitor, IQueryVisitor
{
    #region [ Fields ]

    private readonly StringBuilder filterBuilder = new();
    private readonly StringBuilder orderBuilder = new();
    private int skip = 0;
    private int take = 0;
    private bool hasWhere = false;
    #endregion


    public string ToQueryString()
    {
        var queryParams = new List<string>();
        if (filterBuilder.Length > 0)
            queryParams.Add($"$filter={HttpUtility.UrlEncode(filterBuilder.ToString())}");
        if (orderBuilder.Length > 0)
            queryParams.Add($"$orderby={HttpUtility.UrlEncode(orderBuilder.ToString())}");
        if (skip > 0)
            queryParams.Add($"$skip={skip}");
        if (take != int.MaxValue)
            queryParams.Add($"$top={take}");

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
                    if (hasWhere) filterBuilder.Append(" and ");
                    hasWhere = true;

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
                    if (orderBuilder.Length > 0) orderBuilder.Append(", ");

                    // Extract property name from lambda
                    var lambda = (LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand;
                    var propertyName = GetPropertyName(lambda.Body);

                    orderBuilder.Append(propertyName);
                    if (node.Method.Name == "OrderByDescending")
                    {
                        orderBuilder.Append(" desc");
                    }
                }
                return Visit(node.Arguments[0]);

            case "ThenBy":
            case "ThenByDescending":
                if (node.Arguments.Count >= 2)
                {
                    if (orderBuilder.Length > 0) orderBuilder.Append(", ");

                    var lambda = (LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand;
                    var propertyName = GetPropertyName(lambda.Body);

                    orderBuilder.Append(propertyName);
                    if (node.Method.Name == "ThenByDescending")
                    {
                        orderBuilder.Append(" desc");
                    }
                }
                return Visit(node.Arguments[0]);

            case "Skip":
                if (node.Arguments.Count >= 2 && node.Arguments[1] is ConstantExpression skipConst)
                {
                    skip = (int)skipConst.Value!;
                }
                return Visit(node.Arguments[0]);

            case "Take":
                if (node.Arguments.Count >= 2 && node.Arguments[1] is ConstantExpression takeConst)
                {
                    take = (int)takeConst.Value!;
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
                    filterBuilder.Append($"contains({propertyName}, {value})");
                    return node;
                case "StartsWith":
                    filterBuilder.Append($"startswith({propertyName}, {value})");
                    return node;
                case "EndsWith":
                    filterBuilder.Append($"endswith({propertyName}, {value})");
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
            filterBuilder.Append(" and ");
            Visit(node.Right);
            return node;
        }
        else if (node.NodeType == ExpressionType.OrElse)
        {
            filterBuilder.Append("(");
            Visit(node.Left);
            filterBuilder.Append(" or ");
            Visit(node.Right);
            filterBuilder.Append(")");
            return node;
        }

        // Handle comparison operators
        var left = GetPropertyName(node.Left);
        var right = GetConstantValue(node.Right);

        var operatorStr = node.NodeType switch
        {
            ExpressionType.Equal => "eq",
            ExpressionType.NotEqual => "ne",
            ExpressionType.GreaterThan => "gt",
            ExpressionType.GreaterThanOrEqual => "ge",
            ExpressionType.LessThan => "lt",
            ExpressionType.LessThanOrEqual => "le",
            _ => "eq"
        };

        filterBuilder.Append($"{left} {operatorStr} {right}");
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
                    var propertyName = GetPropertyName(member);
                    var value = GetConstantValue(method.Arguments[0]);
                    filterBuilder.Append($"contains({propertyName}, {value})");
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
