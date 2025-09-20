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
    /// Extension method for ThenInclude on IEnumerable&lt;T&gt; properties
    /// Allows navigation from a collection property to properties of the collection elements
    /// </summary>
    /// <typeparam name="TEntity">The root entity type being queried</typeparam>
    /// <typeparam name="TElement">The element type of the collection</typeparam>
    /// <typeparam name="TProperty">The type of the property to include from each collection element</typeparam>
    /// <param name="includable">The current includable instance</param>
    /// <param name="navigationPropertyPath">Expression pointing to the property on each collection element</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    /// <example>
    /// <code>
    /// // For a Workout that has List&lt;WeekPlan&gt; WeekPlans property:
    /// .Include(x => x.WeekPlans)                    // Returns IIncludable&lt;Workout, List&lt;WeekPlan&gt;&gt;
    ///     .ThenInclude(x => x.WeekPlanSets)         // Uses this extension: x is WeekPlan, returns IIncludable&lt;Workout, List&lt;WeekPlanSet&gt;&gt;
    /// </code>
    /// </example>
    public static IIncludable<TEntity, IEnumerable<TProperty>> ThenInclude<TEntity, TElement, TProperty>(
        this IIncludable<TEntity, IEnumerable<TElement>> includable,
        Expression<Func<TElement, TProperty>> navigationPropertyPath)
    {
        // Cast to access the underlying ThenInclude method
        // The base ThenInclude method expects Expression<Func<IEnumerable<TElement>, TProperty>>
        // but we're providing Expression<Func<TElement, TProperty>>
        // This is handled by the expression parsing in ExpressionHelper
        return includable.ThenInclude(CreateCollectionNavigationExpression<TElement, TProperty>(navigationPropertyPath));
    }

    /// <summary>
    /// Extension method for ThenInclude on List&lt;T&gt; properties (commonly used in Entity Framework)
    /// </summary>
    /// <typeparam name="TEntity">The root entity type being queried</typeparam>
    /// <typeparam name="TElement">The element type of the list</typeparam>
    /// <typeparam name="TProperty">The type of the property to include from each list element</typeparam>
    /// <param name="includable">The current includable instance</param>
    /// <param name="navigationPropertyPath">Expression pointing to the property on each list element</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    public static IIncludable<TEntity, List<TProperty>> ThenInclude<TEntity, TElement, TProperty>(
        this IIncludable<TEntity, List<TElement>> includable,
        Expression<Func<TElement, TProperty>> navigationPropertyPath)
    {
        // Create a List-specific expression
        var parameter = Expression.Parameter(typeof(List<TElement>), "list");
        var selectMethod = typeof(System.Linq.Enumerable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TElement), typeof(TProperty));
        
        var selectCall = Expression.Call(selectMethod, parameter, navigationPropertyPath);
        var toListMethod = typeof(System.Linq.Enumerable).GetMethod("ToList")!
            .MakeGenericMethod(typeof(TProperty));
        var toListCall = Expression.Call(toListMethod, selectCall);
        
        var lambda = Expression.Lambda<Func<List<TElement>, List<TProperty>>>(toListCall, parameter);
        return includable.ThenInclude(lambda);
    }

    /// <summary>
    /// Extension method for ThenInclude on ICollection&lt;T&gt; properties
    /// </summary>
    /// <typeparam name="TEntity">The root entity type being queried</typeparam>
    /// <typeparam name="TElement">The element type of the collection</typeparam>
    /// <typeparam name="TProperty">The type of the property to include from each collection element</typeparam>
    /// <param name="includable">The current includable instance</param>
    /// <param name="navigationPropertyPath">Expression pointing to the property on each collection element</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    public static IIncludable<TEntity, ICollection<TProperty>> ThenInclude<TEntity, TElement, TProperty>(
        this IIncludable<TEntity, ICollection<TElement>> includable,
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

/// <summary>
/// Additional extension methods for specific collection types that might be encountered
/// </summary>
public static class SpecializedCollectionExtensions
{
    /// <summary>
    /// Extension method for arrays
    /// </summary>
    public static IIncludable<TEntity, TProperty[]> ThenInclude<TEntity, TElement, TProperty>(
        this IIncludable<TEntity, TElement[]> includable,
        Expression<Func<TElement, TProperty>> navigationPropertyPath)
    {
        // For arrays, we create a parameter of array type
        var parameter = Expression.Parameter(typeof(TElement[]), "array");
        var selectMethod = typeof(System.Linq.Enumerable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TElement), typeof(TProperty));
        
        var selectCall = Expression.Call(selectMethod, parameter, navigationPropertyPath);
        var toArrayMethod = typeof(System.Linq.Enumerable).GetMethod("ToArray")!
            .MakeGenericMethod(typeof(TProperty));
        var toArrayCall = Expression.Call(toArrayMethod, selectCall);
        
        var lambda = Expression.Lambda<Func<TElement[], TProperty[]>>(toArrayCall, parameter);
        return includable.ThenInclude(lambda);
    }

    /// <summary>
    /// Extension method for IQueryable&lt;T&gt; (though less common in REST API contexts)
    /// </summary>
    public static IIncludable<TEntity, IQueryable<TProperty>> ThenInclude<TEntity, TElement, TProperty>(
        this IIncludable<TEntity, IQueryable<TElement>> includable,
        Expression<Func<TElement, TProperty>> navigationPropertyPath)
    {
        // For IQueryable, we create a parameter of IQueryable type
        var parameter = Expression.Parameter(typeof(IQueryable<TElement>), "queryable");
        var selectMethod = typeof(System.Linq.Queryable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TElement), typeof(TProperty));
        
        var selectCall = Expression.Call(selectMethod, parameter, navigationPropertyPath);
        var lambda = Expression.Lambda<Func<IQueryable<TElement>, IQueryable<TProperty>>>(selectCall, parameter);
        return includable.ThenInclude(lambda);
    }
}