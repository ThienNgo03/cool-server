using System.Linq.Expressions;
using System.Reflection;

namespace Library.Queryable.Include;

/// <summary>
/// Helper class to parse LINQ expressions into include strings for any entity type
/// Moved from Workouts.Query to be reusable across all resource-oriented entities
/// </summary>
public static class ExpressionHelper
{
    /// <summary>
    /// Converts a lambda expression to a property path string
    /// Example: x => x.WeekPlans becomes "weekplans"
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <typeparam name="TProperty">Property type</typeparam>
    /// <param name="expression">The lambda expression</param>
    /// <returns>Property path as lowercase string</returns>
    public static string GetPropertyPath<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        switch (expression.Body)
        {
            case MemberExpression memberExpression:
                return GetMemberPath(memberExpression);

            case MethodCallExpression methodCallExpression:
                // Handle cases like collection => collection.Select(x => x.WeekPlanSets).ToList()
                return ExtractPropertyFromMethodCall(methodCallExpression);

            case UnaryExpression unaryExpression when unaryExpression.Operand is MemberExpression unaryMember:
                // Handle cases like x => (object)x.Property
                return GetMemberPath(unaryMember);
        }

        throw new ArgumentException($"Invalid expression type: {expression.Body.GetType()} - Expression: {expression}", nameof(expression));
    }

    /// <summary>
    /// Extracts property name from method call expressions like collection.Select(x => x.Property).ToList()
    /// </summary>
    /// <param name="methodCall">The method call expression</param>
    /// <returns>Property name in lowercase</returns>
    private static string ExtractPropertyFromMethodCall(MethodCallExpression methodCall)
    {
        // Handle Select(...).ToList() patterns
        if (methodCall.Method.Name == "ToList" && methodCall.Arguments.Count == 1)
        {
            if (methodCall.Arguments[0] is MethodCallExpression selectCall && 
                selectCall.Method.Name == "Select" && 
                selectCall.Arguments.Count == 2)
            {
                // Get the lambda expression from Select: x => x.Property
                if (selectCall.Arguments[1] is LambdaExpression lambda &&
                    lambda.Body is MemberExpression memberExpr)
                {
                    return memberExpr.Member.Name.ToLowerInvariant();
                }
            }
        }

        // Handle direct member access method calls
        if (methodCall.Object is MemberExpression objMember)
        {
            return GetMemberPath(objMember);
        }

        throw new ArgumentException($"Cannot extract property from method call: {methodCall}");
    }

    /// <summary>
    /// Extracts the member path from a member expression
    /// </summary>
    /// <param name="memberExpression">The member expression</param>
    /// <returns>Property path as lowercase string</returns>
    private static string GetMemberPath(MemberExpression memberExpression)
    {
        var path = new List<string>();
        var current = memberExpression;

        while (current != null)
        {
            if (current.Member is PropertyInfo property)
            {
                path.Insert(0, property.Name.ToLowerInvariant());
            }
            else
            {
                throw new ArgumentException($"Expression must be a property access, but found: {current.Member.MemberType}");
            }

            current = current.Expression as MemberExpression;
        }

        return string.Join(".", path);
    }

    /// <summary>
    /// Combines multiple include paths into a single comma-separated string
    /// </summary>
    /// <param name="includePaths">Collection of include paths</param>
    /// <returns>Combined include string</returns>
    public static string CombineIncludes(IEnumerable<string> includePaths)
    {
        return string.Join(",", includePaths.Where(path => !string.IsNullOrWhiteSpace(path)));
    }

    /// <summary>
    /// Combines existing includes with a new property path for regular Include operations
    /// </summary>
    /// <param name="existingIncludes">Current list of includes</param>
    /// <param name="newPropertyPath">New property path to add</param>
    /// <returns>New list with the added include</returns>
    public static List<string> CombineIncludes(List<string> existingIncludes, string newPropertyPath)
    {
        var result = new List<string>(existingIncludes);
        if (!string.IsNullOrWhiteSpace(newPropertyPath))
        {
            result.Add(newPropertyPath);
        }
        return result;
    }

    /// <summary>
    /// Combines existing includes with a new property path for ThenInclude operations
    /// </summary>
    /// <param name="existingIncludes">Current list of includes</param>
    /// <param name="newPropertyPath">New property path to add</param>
    /// <param name="isTheneInclude">True if this is a ThenInclude operation</param>
    /// <returns>New list with the combined include</returns>
    public static List<string> CombineIncludes(List<string> existingIncludes, string newPropertyPath, bool isTheneInclude)
    {
        var result = new List<string>(existingIncludes);
        
        if (isTheneInclude && result.Count > 0 && !string.IsNullOrWhiteSpace(newPropertyPath))
        {
            // For ThenInclude, append to the last include path
            var lastInclude = result[result.Count - 1];
            result[result.Count - 1] = $"{lastInclude}.{newPropertyPath}";
        }
        else if (!string.IsNullOrWhiteSpace(newPropertyPath))
        {
            // For regular Include, add as new path
            result.Add(newPropertyPath);
        }
        
        return result;
    }
}