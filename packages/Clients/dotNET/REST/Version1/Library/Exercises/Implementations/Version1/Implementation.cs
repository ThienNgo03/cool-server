using Refit;
using System.Diagnostics;

namespace Library.Exercises.Implementations.Version1;

public class Implementation : Interface
{
    #region [ Fields ]

    private readonly IRefitInterface refitInterface;
    #endregion

    #region [ CTors ]

    public Implementation(IRefitInterface refitInterface)
    {
        this.refitInterface = refitInterface;
    }
    #endregion

    #region [ Methods ]

    public async Task<Models.PaginationResults.Model<Model>> GetAsync(GET.Parameters parameters)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await this.refitInterface.GET(parameters);

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            if(response is null || response.Content is null)
            {
                return new()
                {
                    Size = parameters.PageSize,
                    Index = parameters.PageIndex,
                    Items = new List<Model>(),
                    Total = 0,
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new()
                {
                    Size = parameters.PageSize,
                    Index = parameters.PageIndex,
                    Items = new List<Model>(),
                    Total = 0,
                };
            }

            List<Model> items = new(); 
            var data = response.Content.Items;
            if(data is null || !data.Any())
            {

                return new()
                {
                    Size = parameters.PageSize,
                    Index = parameters.PageIndex,
                    Items = new List<Model>(),
                    Total = 0,
                };
            }

            return response.Content;
        }
        catch (ApiException ex)
        {
            throw new HttpRequestException(
                $"Server returned error {ex.StatusCode}: {ex.Message}",
                ex,
                ex.StatusCode);
        }
    }

    public async Task CreateAsync(POST.Payload payload)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await this.refitInterface.POST(payload);

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
        }
        catch (ApiException ex)
        {
            stopwatch.Stop();
        }
    }

    public async Task UpdateAsync(PUT.Payload payload)
    {
        try
        {
            var response = await this.refitInterface.PUT(payload);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException();
        }
    }

    public async Task DeleteAsync(DELETE.Parameters parameters)
    {
        try
        {
            var response = await this.refitInterface.DELETE(parameters);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
