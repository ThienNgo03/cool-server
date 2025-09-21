using System.Linq.Expressions;

namespace Library.Queryable.Include;

public abstract class IncludeBuilder<TEntity, TProperty> : IIncludable<TEntity, TProperty>
{
    protected readonly List<string> _includes;

    protected IncludeBuilder()
    {
        _includes = new List<string>();
    }

    protected IncludeBuilder(IEnumerable<string> includes)
    {
        _includes = new List<string>(includes);
    }

    public IIncludable<TEntity, TNewProperty> Include<TNewProperty>(Expression<Func<TEntity, TNewProperty>> navigationPropertyPath)
    {
        var propertyPath = ExpressionHelper.GetPropertyPath(navigationPropertyPath);
        var newIncludes = ExpressionHelper.CombineIncludes(_includes, propertyPath);

        return CreateIncludeBuilder<TNewProperty>(newIncludes);
    }

    public IIncludable<TEntity, TNewProperty> ThenInclude<TNewProperty>(Expression<Func<TProperty, TNewProperty>> navigationPropertyPath)
    {
        var propertyPath = ExpressionHelper.GetPropertyPath(navigationPropertyPath);
        var newIncludes = ExpressionHelper.CombineIncludes(_includes, propertyPath, isTheneInclude: true);

        return CreateIncludeBuilder<TNewProperty>(newIncludes);
    }

    public IIncludable<TEntity, ICollection<TNewProperty>> ThenInclude<TNewProperty>(Expression<Func<TProperty, IEnumerable<TNewProperty>>> navigationPropertyPath)
        where TNewProperty : class
    {
        var propertyPath = ExpressionHelper.GetPropertyPath(navigationPropertyPath);
        var newIncludes = ExpressionHelper.CombineIncludes(_includes, propertyPath, isTheneInclude: true);

        return CreateIncludeBuilder<ICollection<TNewProperty>>(newIncludes);
    }

    public abstract Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<TEntity>>> AllAsync<TParameters>(TParameters parameters);

    protected abstract IIncludable<TEntity, TNewProperty> CreateIncludeBuilder<TNewProperty>(List<string> includes);

    public string GetIncludesString() => string.Join(",", _includes);
}

internal class GenericIncludeBuilder<TEntity, TProperty> : IncludeBuilder<TEntity, TProperty>
    where TEntity : class
{
    private readonly ResourceQueryable<TEntity> _service;

    public GenericIncludeBuilder(ResourceQueryable<TEntity> service) : base()
    {
        _service = service;
    }

    public GenericIncludeBuilder(ResourceQueryable<TEntity> service, IEnumerable<string> includes) : base(includes)
    {
        _service = service;
    }

    protected override IIncludable<TEntity, TNewProperty> CreateIncludeBuilder<TNewProperty>(List<string> includes)
    {
        return new GenericIncludeBuilder<TEntity, TNewProperty>(_service, includes);
    }

    public override async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<TEntity>>> AllAsync<TParameters>(TParameters parameters)
    {
        var includeString = GetIncludesString();

        if (!string.IsNullOrEmpty(includeString))
        {
            var includeProperty = typeof(TParameters).GetProperty("Include");
            if (includeProperty != null && includeProperty.CanWrite)
            {
                includeProperty.SetValue(parameters, includeString);
            }
        }

        return await _service.AllAsync(parameters);
    }
}
