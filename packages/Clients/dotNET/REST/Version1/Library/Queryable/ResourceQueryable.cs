using System.Linq.Expressions;
using Library.Queryable.Include;

namespace Library.Queryable;

public abstract class ResourceQueryable<TEntity> : IResourceQueryable<TEntity>
    where TEntity : class
{
    public abstract Task<Models.Response.Model<Models.PaginationResults.Model<TEntity>>> AllAsync<TParameters>(TParameters parameters);
    public virtual IIncludable<TEntity, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> navigationProperty)
    {
        var builder = new GenericIncludeBuilder<TEntity, TProperty>(this);
        return builder.Include(navigationProperty);
    }
}