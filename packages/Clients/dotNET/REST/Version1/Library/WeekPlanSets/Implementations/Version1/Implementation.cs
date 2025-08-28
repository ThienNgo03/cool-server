
using Library.Models.Patch;
using Refit;
using System.Diagnostics;

namespace Library.WeekPlanSets.Implementations.Version1;

public class Implementation(IRefitInterface refitInterface) : Interface
{
    private readonly IRefitInterface refitInterface = refitInterface;
    public async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>> AllAsync(All.Parameters parameters)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Models.Refit.GET.Parameters refitParameters = new()
        {
            Id = parameters.Id,
            WeekPlanId = parameters.WeekPlanId,
            Value = parameters.Value,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
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
                    Detail = $"Successfully fetched {items.Count} week plan set(s)",
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
                    WeekPlanId = item.WeekPlanId,
                    Value = item.Value,
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

    public async Task CreateAsync(Create.Payload payload)
    {
        try
        {
            var refitPayload = new Models.Refit.POST.Payload
            {
                WeekPlanId = payload.WeekPlanId,
                Value = payload.Value
            };

            var response = await this.refitInterface.POST(refitPayload);
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

    public async Task PatchAsync(Parameters parameters)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Models.Refit.PATCH.Parameters refitParameters = new()
        {
            Id = parameters.Id
        };

        var operations = parameters.Operations.Select(op => new Models.Refit.PATCH.Operation
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

    public async Task UpdateAsync(Update.Payload payload)
    {
        try
        {
            var refitPayload = new Models.Refit.PUT.Payload
            {
                Id = payload.Id,
                WeekPlanId = payload.WeekPlanId,
                Value = payload.Value
            };

            var response = await this.refitInterface.PUT(refitPayload);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException();
        }
    }
}
