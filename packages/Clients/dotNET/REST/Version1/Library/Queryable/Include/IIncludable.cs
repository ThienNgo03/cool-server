using System.Linq.Expressions;

namespace Library.Queryable.Include;

/// <summary>
/// Generic interface for includable queries that supports fluent Include/ThenInclude syntax
/// This interface is designed to work with any entity type and provides type-safe navigation property includes
/// </summary>
/// <typeparam name="TEntity">The root entity type being queried</typeparam>
/// <typeparam name="TProperty">The current property type in the include chain</typeparam>
public interface IIncludable<TEntity, TProperty>
{
    /// <summary>
    /// Includes a related entity from the root entity, starting a new include chain
    /// </summary>
    /// <typeparam name="TNewProperty">The type of the property to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the navigation property from the root entity</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    /// <example>
    /// <code>
    /// .Include(x => x.Exercise)  // Start new include chain from root
    /// </code>
    /// </example>
    IIncludable<TEntity, TNewProperty> Include<TNewProperty>(Expression<Func<TEntity, TNewProperty>> navigationPropertyPath);

    /// <summary>
    /// Includes a related entity from the previously included property, continuing the current include chain
    /// </summary>
    /// <typeparam name="TNewProperty">The type of the property to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the navigation property from the previous include</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    /// <example>
    /// <code>
    /// .Include(x => x.Exercise)
    ///     .ThenInclude(x => x.Muscles)  // Continue from Exercise to Muscles
    /// </code>
    /// </example>
    IIncludable<TEntity, TNewProperty> ThenInclude<TNewProperty>(Expression<Func<TProperty, TNewProperty>> navigationPropertyPath);

    /// <summary>
    /// Collection ThenInclude for navigating from a collection property to another collection
    /// Handles cases like .Include(x => x.WeekPlans).ThenInclude(x => x.WeekPlanSets)
    /// </summary>
    /// <typeparam name="TNewProperty">The type of the collection elements to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the collection navigation property</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    IIncludable<TEntity, ICollection<TNewProperty>> ThenInclude<TNewProperty>(Expression<Func<TProperty, IEnumerable<TNewProperty>>> navigationPropertyPath)
        where TNewProperty : class;

    /// <summary>
    /// Executes the query with all configured includes and returns paginated results
    /// Uses generic parameter type to support different entity parameter types
    /// </summary>
    /// <typeparam name="TParameters">The parameter type specific to the entity</typeparam>
    /// <param name="parameters">Parameters for pagination and filtering</param>
    /// <returns>Paginated results with included navigation properties</returns>
    Task<Models.Response.Model<Models.PaginationResults.Model<TEntity>>> AllAsync<TParameters>(TParameters parameters);
}