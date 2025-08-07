using Library.Queryable;

namespace Library.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> UseQueryStyle<T>(this IQueryable<T> queryable, QueryStyle style)
    {
        if(queryable is ApiQueryable<T> apiQueryable)
        {
            return apiQueryable.UseQueryStyle(style);
        }
        return queryable;
    }
}
