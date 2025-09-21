using System.Collections.Generic;
using System.Linq.Expressions;

namespace Library.Queryable.Include.Extensions;

/// <summary>
/// Extension methods for handling ThenInclude operations on collection properties
/// Allows fluent navigation from collections to their element properties
/// </summary>
public static class CollectionExtensions
{

    /// <summary>
    /// Extension method for ThenInclude on ICollection&lt;T&gt; properties
    /// </summary>
    /// <typeparam name="TEntity">The root entity type being queried</typeparam>
    /// <typeparam name="TElement">The element type of the collection</typeparam>
    /// <typeparam name="TProperty">The type of the property to include from each collection element</typeparam>
    /// <param name="includable">The current includable instance</param>
    /// <param name="navigationPropertyPath">Expression pointing to the property on each collection element</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    public static IIncludable<TEntity, ICollection<TProperty>?> ThenInclude<TEntity, TElement, TProperty>(
        this IIncludable<TEntity, ICollection<TElement>?> includable,
        Expression<Func<TElement, TProperty>> navigationPropertyPath)
    {
        // Create an ICollection-specific expression
        var parameter = Expression.Parameter(typeof(ICollection<TElement>), "collection");
        var selectMethod = typeof(System.Linq.Enumerable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TElement), typeof(TProperty));
        
        var selectCall = Expression.Call(selectMethod, parameter, navigationPropertyPath);
        var toListMethod = typeof(System.Linq.Enumerable).GetMethod("ToList")!
            .MakeGenericMethod(typeof(TProperty));
        var toListCall = Expression.Call(toListMethod, selectCall);
        
        var lambda = Expression.Lambda<Func<ICollection<TElement>, ICollection<TProperty>>>(toListCall, parameter);
        return includable.ThenInclude(lambda);
    }

    /// <summary>
    /// Helper method to create the proper expression for collection navigation
    /// This handles the conversion from element-level expressions to collection-level expressions
    /// </summary>
    /// <typeparam name="TElement">The element type of the collection</typeparam>
    /// <typeparam name="TProperty">The property type to navigate to</typeparam>
    /// <param name="elementExpression">Expression for navigation on a single element</param>
    /// <returns>Expression that can be used with the base ThenInclude method</returns>
    public static Expression<Func<IEnumerable<TElement>, IEnumerable<TProperty>>> CreateCollectionNavigationExpression<TElement, TProperty>(
        Expression<Func<TElement, TProperty>> elementExpression)
    {
        // This method creates an expression that represents:
        // collection => collection.Select(element => elementExpression(element))
        // However, for our purposes, we only need the property path from the element expression
        // The actual conversion is handled in ExpressionHelper.GetPropertyPath
        
        var parameter = Expression.Parameter(typeof(IEnumerable<TElement>), "collection");
        var selectMethod = typeof(System.Linq.Enumerable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TElement), typeof(TProperty));
        
        var selectCall = Expression.Call(selectMethod, parameter, elementExpression);
        
        return Expression.Lambda<Func<IEnumerable<TElement>, IEnumerable<TProperty>>>(selectCall, parameter);
    }
}
