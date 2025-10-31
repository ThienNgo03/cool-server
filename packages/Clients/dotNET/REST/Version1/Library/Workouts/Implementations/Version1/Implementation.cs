

using Refit;
using System.Diagnostics;

namespace Library.Workouts.Implementations.Version1;

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

            if (response is null || response.Content is null)
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
            var serverItems = response.Content.Items;
            if (serverItems is null || !serverItems.Any())
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

    public async Task PostAsync(POST.Payload payload)
    {
        try
        {
            var response = await this.refitInterface.POST(payload);
        }

        catch (ApiException ex)
        {
            throw new HttpRequestException(
                $"Server returned error {ex.StatusCode}: {ex.Message}",
                ex,
                ex.StatusCode);
        }
    }

    public async Task PutAsync(PUT.Payload payload)
    {
        try
        {
            var response = await this.refitInterface.PUT(payload);
        }
        catch (ApiException ex)
        {
            throw new HttpRequestException(
                $"Server returned error {ex.StatusCode}: {ex.Message}",
                ex,
                ex.StatusCode);
        }
    }

    public async Task PatchAsync(Models.Patch.Parameters parameters)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        PATCH.Parameters refitParameters = new()
        {
            Id = parameters.Id
        };

        var operations = parameters.Operations.Select(op => new PATCH.Operation
        {
            op = "replace",
            path = $"/{op.Path}",
            value = op.Value
        }).ToList();

        try
        {
            var result = await refitInterface.PATCH(refitParameters, operations);
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
        }
        catch (ApiException ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            Debug.WriteLine("Error in: " + nameof(PatchAsync));
            Debug.WriteLine("Status code: " + ex.StatusCode);
            Debug.WriteLine("Message: " + ex.Message);
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
            throw new HttpRequestException(
                $"Server returned error {ex.StatusCode}: {ex.Message}",
                ex,
                ex.StatusCode);
        }
    }

    #endregion
}
