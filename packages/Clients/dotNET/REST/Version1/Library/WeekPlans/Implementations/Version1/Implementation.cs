using Library.WeekPlans.All;
using Library.WeekPlans.Create;
using Refit;
using System.Diagnostics;
namespace Library.WeekPlans.Implementations.Version1;

public class Implementation : Interface
{
    #region [Fields]
    private readonly IRefitInterface refitInterface;

    #endregion

    #region [CTors]
    public Implementation(IRefitInterface refitInterface)
    {
        this.refitInterface = refitInterface;
    }
    #endregion

    #region [Methods]
    public async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>> AllAsync(Parameters parameters)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Models.Refit.GET.Parameters refitParameters = new()
        {
            Id = parameters.Id,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            WorkoutId = parameters.WorkoutId,
            DateOfWeek = parameters.DateOfWeek,
            Time = parameters.Time,
            CreatedDate = parameters.CreatedDate,
            LastUpdated = parameters.LastUpdated
        };

        try
        {
            var response = await this.refitInterface.GET(refitParameters);

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            if (response is null || response.Content is null)
            {
                return new()
                {

                    Title = "Couldn't reach to the server",
                    Detail = $"Failed at {nameof(AllAsync)}, after make a request call through refit",
                    Data = null,
                    Duration = duration,
                    IsSuccess = false
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new()
                {
                    Title = $"Error: {response.StatusCode}",
                    Detail = response.Error.Message,
                    Data = null,
                    Duration = duration,
                    IsSuccess = false
                };
            }

            List<Model> items = new();
            var data = response.Content.Items;
            if (data is null || !data.Any())
            {

                return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
                {
                    Title = "Success",
                    Detail = $"Successfully fetched {items.Count} week plan(s)",
                    Duration = duration,
                    IsSuccess = true,
                    Data = new Library.Models.PaginationResults.Model<Model>
                    {
                        Total = items.Count,
                        Index = parameters.PageIndex,
                        Size = parameters.PageSize,
                        Items = items
                    }
                };
            }

            foreach (var item in data)
            {
                items.Add(new()
                {
                    Id = item.Id,
                    WorkoutId = item.WorkoutId,
                    DateOfWeek = item.DateOfWeek,
                    Time = item.Time,
                    CreatedDate = item.CreatedDate,
                    LastUpdated = item.LastUpdated,
                });
            }

            return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
            {
                Title = "Success",
                Detail = $"Successfully fetched {items.Count} week plan(s)",
                Duration = duration,
                IsSuccess = true,
                Data = new Library.Models.PaginationResults.Model<Model>
                {
                    Total = items.Count,
                    Index = parameters.PageIndex,
                    Size = parameters.PageSize,
                    Items = items
                }
            };
        }
        catch (ApiException ex)
        {

            throw new NotImplementedException();
        }
    }

    public async Task CreateAsync(Payload payload)
    {
        try
        {
            var refitPayload = new Models.Refit.POST.Payload
            {
                WorkoutId = payload.WorkoutId,
                DateOfWeek = payload.DateOfWeek,
                Time = payload.Time
            };

            var response = await this.refitInterface.POST(refitPayload);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException();
        }
    }

    public async Task UpdateAsync(Update.Payload payload)
    {
        try
        {
            var refitPayload = new Models.Refit.PUT.Payload
            {
                Id = payload.Id,
                WorkoutId = payload.WorkoutId,
                DateOfWeek = payload.DateOfWeek,
                Time = payload.Time
            };
            var response = await this.refitInterface.PUT(refitPayload);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException();
        }
    }

    public async Task DeleteAsync(Delete.Parameters parameters)
    {
        try
        {
            var refitParameters = new Models.Refit.DELETE.Parameters
            {
                Id = parameters.Id
            };

            var response = await this.refitInterface.DELETE(refitParameters);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
