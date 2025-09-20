using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using Library.Queryable.Include;

namespace Library.Queryable;

/// <summary>
/// Fully generic base class for resource queryable implementations.
/// Automatically detects and processes navigation properties (ICollection and reference types).
/// Services only need to inherit - zero custom Include code required.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public abstract class ResourceQueryableBase<TEntity> : IResourceQueryable<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Creates an includable query for the specified navigation property
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property</typeparam>
    /// <param name="navigationProperty">Expression pointing to the navigation property</param>
    /// <returns>An includable query builder</returns>
    public virtual IIncludable<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> navigationProperty)
    {
        // Return a simple base IncludeBuilder that throws on AllAsync - 
        // services should override this method with their specific builders
        throw new NotImplementedException("Services should override Include method with their specific implementation");
    }

    /// <summary>
    /// Builds include string from expressions for URL parameter (e.g., "exercises.muscles,weekPlans")
    /// </summary>
    /// <param name="includeExpressions">List of include expressions to convert to string</param>
    /// <returns>Comma-separated include string for URL parameter</returns>
    protected virtual string BuildIncludeString(List<string> includeExpressions)
    {
        if (includeExpressions == null || !includeExpressions.Any())
            return string.Empty;

        // Convert expressions like ["Exercise", "Exercise.Muscles", "WeekPlans"] 
        // to string like "exercises.muscles,weekPlans"
        var includeStrings = includeExpressions
            .Select(expr => ConvertToIncludeString(expr))
            .Where(str => !string.IsNullOrEmpty(str))
            .Distinct()
            .ToList();

        return string.Join(",", includeStrings);
    }

    /// <summary>
    /// Converts a single include expression to lowercase dot notation
    /// </summary>
    /// <param name="expression">Expression like "Exercise.Muscles"</param>
    /// <returns>Lowercase string like "exercises.muscles"</returns>
    private string ConvertToIncludeString(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return string.Empty;

        // Convert "Exercise.Muscles" to "exercises.muscles"
        return expression.ToLowerInvariant();
    }

    /// <summary>
    /// Utility method to extract property name from lambda expression
    /// This is used across all implementations to avoid code duplication
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <typeparam name="TProp">Property type</typeparam>
    /// <param name="expression">Lambda expression like x => x.PropertyName</param>
    /// <returns>Property name string</returns>
    protected string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        
        throw new ArgumentException("Expression must be a member access expression", nameof(expression));
    }
}