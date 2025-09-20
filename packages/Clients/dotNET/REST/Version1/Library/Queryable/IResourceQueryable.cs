using System.Linq.Expressions;
using Library.Queryable.Include;

namespace Library.Queryable;

public interface IResourceQueryable<TEntity>
{
    Task<Models.Response.Model<Models.PaginationResults.Model<TEntity>>> AllAsync<TParameters>(TParameters parameters);
    IIncludable<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath);
}