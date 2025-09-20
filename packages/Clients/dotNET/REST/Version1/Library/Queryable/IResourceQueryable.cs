using System.Linq.Expressions;
using Library.Queryable.Include;

namespace Library.Queryable;

/// <summary>
/// Base interface for resource-oriented entities that support queryable operations
/// This should only be implemented by entities that represent database tables/models
/// with CRUD operations and navigation properties (e.g., Workouts, Users, Exercises)
/// 
/// DO NOT use this for action-oriented services (e.g., Authentication, Token services)
/// </summary>
/// <typeparam name="TEntity">The entity model type</typeparam>
public interface IResourceQueryable<TEntity>
{
    /// <summary>
    /// Starts an include chain for related entities using fluent syntax
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the navigation property</param>
    /// <returns>An includable interface for chaining additional includes</returns>
    /// <example>
    /// <code>
    /// var result = await service
    ///     .Include(x => x.RelatedEntity)
    ///         .ThenInclude(x => x.NestedEntity)
    ///     .Include(x => x.AnotherEntity)
    ///     .AllAsync(parameters);
    /// </code>
    /// </example>
    IIncludable<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath);
}