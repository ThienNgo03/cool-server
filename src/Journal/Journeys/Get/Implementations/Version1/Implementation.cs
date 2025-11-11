namespace Journal.Journeys.Get.Implementations.Version1;

public class Implementation : Interface
{
    #region [ Fields ]

    private readonly JournalDbContext _context;
    #endregion

    #region [ CTors ]

    public Implementation(JournalDbContext context)
    {
        _context = context;
    }
    #endregion
    public async Task<Storage> GetStorage(Parameters parameters)
    {
        var query = _context.Journeys.AsQueryable(); //lấy table ra, nhưng chưa đâm xuống Database
                                                     //Query ID
        if (parameters.id.HasValue)
            query = query.Where(x => x.Id == parameters.id);
        //Query Content
        if (!string.IsNullOrEmpty(parameters.content))
            query = query.Where(x => x.Content.Contains(parameters.content));
        //Query Location
        if (!string.IsNullOrEmpty(parameters.location))
            query = query.Where(x => x.Location.Contains(parameters.location));
        //Query Date
        if (parameters.date.HasValue)
            query = query.Where(x => x.Date == parameters.date);
        //chia trang
        if (parameters.pageSize.HasValue && parameters.pageIndex.HasValue && parameters.pageSize > 0 && parameters.pageIndex >= 0)
            query = query.Skip(parameters.pageIndex.Value * parameters.pageSize.Value).Take(parameters.pageSize.Value);

        var result = await query.AsNoTracking().ToListAsync();
        Storage storage = new Storage
        {
            Data = result.Select(x => new Data
            {
                Id = x.Id,
                Content = x.Content,
                Location = x.Location,
                Date = x.Date
            }).ToList()
        };
        return storage;
    }

    public Task<ProcessData.Model> ProcessStorage(Storage storage)
    {
        ProcessData.Model processData = new ProcessData.Model
        {
            PageIndex = storage.PageIndex,
            PageSize = storage.PageSize,
            IsNotesIncluded = storage.IsNotesIncluded
        };

        var extendedData = storage.Data.Select(x => new ProcessData.ExtendedData
        {
            Id = x.Id,
            Content = x.Content,
            Location = x.Location,
            Date = x.Date,
            Weather = "Mua lon"
        }).ToList();

        processData.Data = extendedData;
        return Task.FromResult(processData);
    }

    public Task<Result> CreateResult(ProcessData.Model processData)
    {
        Result result = new Result
        {
            Data = processData.Data,
            PageIndex = processData.PageIndex,
            PageSize = processData.PageSize,
            IsNotesIncluded = processData.IsNotesIncluded
        };
        return Task.FromResult(result);
    }

}
