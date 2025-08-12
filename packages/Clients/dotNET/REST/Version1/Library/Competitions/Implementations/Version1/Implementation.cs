using Library.Competitions.All;
using Library.Competitions.Create;
using Refit;
using System.Diagnostics;

namespace Library.Competitions.Implementations.Version1;

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

    public async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>> AllAsync(Parameters parameters)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Models.Refit.GET.Parameters refitParameters = new()
        {
            Id = parameters.Id,
            Title = parameters.Title,
            Description = parameters.Description,
            ExerciseId = parameters.ExerciseId,
            Location = parameters.Location,
            DateTime = parameters.DateTime,
            CreatedDate = parameters.CreatedDate,
            Type = parameters.Type,
            PageSize = parameters.PageSize , 
            PageIndex = parameters.PageIndex 
        };

        try
        {
            var response = await this.refitInterface.GET(refitParameters);

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            if(response is null || response.Content is null)
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
                    Title =$"Error: {response.StatusCode}",
                    Detail = response.Error.Message,
                    Data = null,
                    Duration = duration,
                    IsSuccess = false
                };
            }

            List<Model> items = []; 
            var data = response.Content.Items;
            if(data is null || data.Count == 0)
            {

                return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
                {
                    Title = "Success",
                    Detail = $"Successfully fetched {items.Count} competition(s)",
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
                    Id = item.Id ,
                    Title = item.Title ,
                    Description = item.Description ,
                    ParticipantIds = item.ParticipantIds ,
                    ExerciseId = item.ExerciseId,
                    Location = item.Location ,
                    DateTime = item.DateTime,
                    CreatedDate = item.CreatedDate,
                    Type = item.Type
                });
            }

            return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
            {
                Title = "Success",
                Detail = $"Successfully fetched {items.Count} exercise(s)",
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

            throw new NotImplementedException(ex.Message);
        }
    }

    public async Task CreateAsync(Payload payload)
    {
        /*Stopwatch stopwatch = Stopwatch.StartNew();*/
        try
        {
            var refitPayload = new Models.Refit.POST.Payload
            {
                Title = payload.Title,
                Description = payload.Description,
                Location = payload.Location,
                ExerciseId = payload.ExerciseId,
                DateTime = payload.DateTime,
                Type = payload.Type
            };

            var response = await this.refitInterface.POST(refitPayload);

            /*stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;*/
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
                Title = payload.Title,
                Description = payload.Description,
                Location = payload.Location,
                ExerciseId = payload.ExerciseId,
                DateTime = payload.DateTime,
                Type = payload.Type
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
                Id = parameters.Id,
                DeleteSoloPool = parameters.DeleteSoloPool,
                DeleteTeamPool = parameters.DeleteTeamPool,
            };

            var response = await this.refitInterface.DELETE(refitParameters);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException(ex.Message);
        }
    }
    #endregion
}
