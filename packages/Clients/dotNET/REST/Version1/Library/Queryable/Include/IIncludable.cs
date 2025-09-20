using System.Linq.Expressions;

namespace Library.Queryable.Include;

public interface IIncludable<TEntity, TProperty>
{
    IIncludable<TEntity, TNewProperty?> Include<TNewProperty>(Expression<Func<TEntity, TNewProperty?>> navigationPropertyPath);

    IIncludable<TEntity, TNewProperty?> ThenInclude<TNewProperty>(Expression<Func<TProperty, TNewProperty?>> navigationPropertyPath);

    IIncludable<TEntity, ICollection<TNewProperty>?> ThenInclude<TNewProperty>(Expression<Func<TProperty, IEnumerable<TNewProperty>?>> navigationPropertyPath)
        where TNewProperty : class;

    Task<Models.Response.Model<Models.PaginationResults.Model<TEntity>>> AllAsync<TParameters>(TParameters parameters);
}