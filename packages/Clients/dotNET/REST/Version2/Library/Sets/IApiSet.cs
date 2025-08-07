namespace Library.Sets;

public interface IApiSet<T> : IQueryable<T>
{
    Task<T?> FindAsync(string id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(string id);
}
