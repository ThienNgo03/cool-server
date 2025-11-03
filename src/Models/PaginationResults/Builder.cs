namespace Journal.Models.PaginationResults;

public class Builder<T>
{
    private readonly Model<T> _paginationResults = new();

    public Builder<T> WithAll(int all)
    {
        _paginationResults.All = all;
        return this;
    }

    public Builder<T> WithIndex(int? index)
    {
        _paginationResults.Index = index;
        return this;
    }

    public Builder<T> WithSize(int? size)
    {
        _paginationResults.Size = size;
        return this;
    }

    public Builder<T> WithTotal(int total)
    {
        _paginationResults.Total = total;
        return this;
    }

    public Builder<T> WithItems(ICollection<T>? items)
    {
        _paginationResults.Items = items;
        return this;
    }

    public Model<T> Build()
    {
        return _paginationResults;
    }
}