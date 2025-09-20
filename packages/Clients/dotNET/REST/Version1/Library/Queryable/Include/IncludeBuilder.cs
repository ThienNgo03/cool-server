using System.Linq.Expressions;
using Library.Queryable.Include.Extensions;

namespace Library.Queryable.Include;

/// <summary>
/// Abstract base class that implements the core Include/ThenInclude logic for any entity type
/// Derived classes only need to implement entity-specific query execution methods
/// </summary>
/// <typeparam name="TEntity">The root entity type being queried</typeparam>
/// <typeparam name="TProperty">The current property type in the include chain</typeparam>
public abstract class IncludeBuilder<TEntity, TProperty> : IIncludable<TEntity, TProperty>
{
    /// <summary>
    /// List of include strings that will be passed to the API (e.g., "weekplans.weekplansets")
    /// </summary>
    protected readonly List<string> _includes;

    /// <summary>
    /// Initializes a new instance with an empty include list
    /// </summary>
    protected IncludeBuilder()
    {
        _includes = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance with existing includes
    /// </summary>
    /// <param name="includes">Existing include strings</param>
    protected IncludeBuilder(IEnumerable<string> includes)
    {
        _includes = new List<string>(includes);
    }

    /// <summary>
    /// Includes a related entity from the root entity, starting a new include chain
    /// </summary>
    /// <typeparam name="TNewProperty">The type of the property to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the navigation property from the root entity</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    public IIncludable<TEntity, TNewProperty> Include<TNewProperty>(Expression<Func<TEntity, TNewProperty>> navigationPropertyPath)
    {
        var propertyPath = ExpressionHelper.GetPropertyPath(navigationPropertyPath);
        var newIncludes = ExpressionHelper.CombineIncludes(_includes, propertyPath);
        
        return CreateIncludeBuilder<TNewProperty>(newIncludes);
    }

    /// <summary>
    /// Includes a related entity from the previously included property, continuing the current include chain
    /// </summary>
    /// <typeparam name="TNewProperty">The type of the property to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the navigation property from the previous include</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    public IIncludable<TEntity, TNewProperty> ThenInclude<TNewProperty>(Expression<Func<TProperty, TNewProperty>> navigationPropertyPath)
    {
        var propertyPath = ExpressionHelper.GetPropertyPath(navigationPropertyPath);
        var newIncludes = ExpressionHelper.CombineIncludes(_includes, propertyPath, isTheneInclude: true);
        
        return CreateIncludeBuilder<TNewProperty>(newIncludes);
    }

    /// <summary>
    /// Collection ThenInclude for navigating from a collection property to another collection
    /// </summary>
    /// <typeparam name="TNewProperty">The type of the collection elements to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the collection navigation property</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    public IIncludable<TEntity, ICollection<TNewProperty>> ThenInclude<TNewProperty>(Expression<Func<TProperty, IEnumerable<TNewProperty>>> navigationPropertyPath)
        where TNewProperty : class
    {
        var propertyPath = ExpressionHelper.GetPropertyPath(navigationPropertyPath);
        var newIncludes = ExpressionHelper.CombineIncludes(_includes, propertyPath, isTheneInclude: true);
        
        return CreateIncludeBuilder<ICollection<TNewProperty>>(newIncludes);
    }

    /// <summary>
    /// Abstract method for executing the query with includes - must be implemented by derived classes
    /// </summary>
    /// <typeparam name="TParameters">The parameter type specific to the entity</typeparam>
    /// <param name="parameters">Parameters for pagination and filtering</param>
    /// <returns>Paginated results with included navigation properties</returns>
    public abstract Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<TEntity>>> AllAsync<TParameters>(TParameters parameters);

    /// <summary>
    /// Factory method that derived classes must implement to create new builder instances
    /// This ensures type safety while allowing each entity to return its specific builder type
    /// </summary>
    /// <typeparam name="TNewProperty">The property type for the new builder</typeparam>
    /// <param name="includes">The include strings for the new builder</param>
    /// <returns>A new builder instance of the appropriate derived type</returns>
    protected abstract IIncludable<TEntity, TNewProperty> CreateIncludeBuilder<TNewProperty>(List<string> includes);

    /// <summary>
    /// Gets the current list of include strings for API consumption
    /// </summary>
    /// <returns>List of include paths (e.g., ["weekplans.weekplansets", "exercises.muscles"])</returns>
    public List<string> GetIncludes() => new List<string>(_includes);

    /// <summary>
    /// Gets the includes as a comma-separated string for API query parameters
    /// </summary>
    /// <returns>Comma-separated include string (e.g., "weekplans.weekplansets,exercises.muscles")</returns>
    public string GetIncludesString() => string.Join(",", _includes);
}