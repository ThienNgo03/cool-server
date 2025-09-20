using System.Linq.Expressions;
using Library.Queryable;
using Library.Queryable.Include;

namespace Library.Workouts;

public interface Interface : IResourceQueryable<Model>
{
    Task<Models.Response.Model<Models.PaginationResults.Model<Model>>> AllAsync(All.Parameters parameters);
    Task CreateAsync(Create.Payload payload);
    Task UpdateAsync(Update.Payload payload);
    Task DeleteAsync(Delete.Parameters parameters);
    
    /// <summary>
    /// Starts a fluent Include chain for navigation properties
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to include</typeparam>
    /// <param name="navigationPropertyPath">Expression pointing to the navigation property</param>
    /// <returns>Generic includable interface for fluent chaining and execution</returns>
    new IIncludable<Model, TProperty> Include<TProperty>(Expression<Func<Model, TProperty>> navigationPropertyPath);
}
