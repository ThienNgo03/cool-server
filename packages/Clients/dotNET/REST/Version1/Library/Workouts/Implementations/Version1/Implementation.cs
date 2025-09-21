using Library.Workouts.All;
using Library.Workouts.Create;
using Library.Queryable;

using Refit;
using System.Diagnostics;

namespace Library.Workouts.Implementations.Version1;

public class Implementation : ResourceQueryable<Model>, Interface
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

    public override async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>> AllAsync<TParameters>(TParameters parameters)
    {
        if (parameters is not All.Parameters processedParams)
        {
            throw new ArgumentException($"Expected {typeof(All.Parameters).Name} but got {typeof(TParameters).Name}", nameof(parameters));
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        Models.Refit.GET.Parameters refitParameters = new()
        {
            Id = processedParams.Id,
            PageIndex = processedParams.PageIndex,
            PageSize = processedParams.PageSize,
            ExerciseId = processedParams.ExerciseId,
            UserId = processedParams.UserId,
            CreatedDate = processedParams.CreatedDate,
            LastUpdated = processedParams.LastUpdated,
            Include = processedParams.Include
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
                    Detail = $"Successfully fetched {items.Count} workout(s)",
                    Duration = duration,
                    IsSuccess = true,
                    Data = new Library.Models.PaginationResults.Model<Model>
                    {
                        Total = items.Count,
                        Index = processedParams.PageIndex,
                        Size = processedParams.PageSize,
                        Items = items
                    }
                };
            }

            foreach (var item in data)
            {
                items.Add(new()
                {
                    Id = item.Id,
                    ExerciseId = item.ExerciseId,
                    UserId = item.UserId,
                    CreatedDate = item.CreatedDate,
                    LastUpdated = item.LastUpdated,
                    Exercise = item.Exercise is null ? null : new Exercise
                    {
                        Id = item.Exercise.Id,
                        Name = item.Exercise.Name,
                        Description = item.Exercise.Description,
                        Type = item.Exercise.Type,
                        CreatedDate = item.Exercise.CreatedDate,
                        LastUpdated = item.Exercise.LastUpdated,
                        Muscles = item.Exercise.Muscles?.Select(em => new Muscle
                        {
                            Id = em.Id,
                            Name = em.Name,
                            CreatedDate = em.CreatedDate,
                            LastUpdated = em.LastUpdated
                        }).ToList()
                    },
                    WeekPlans = item.WeekPlans?.Select(wp => new WeekPlan
                    {
                        Id = wp.Id,
                        WorkoutId = wp.WorkoutId,
                        DateOfWeek = wp.DateOfWeek,
                        Time = wp.Time,
                        CreatedDate = wp.CreatedDate,
                        LastUpdated = wp.LastUpdated,
                        WeekPlanSets = wp.WeekPlanSets?.Select(wps => new WeekPlanSet
                        {
                            Id = wps.Id,
                            WeekPlanId = wps.WeekPlanId,
                            Value = wps.Value,
                            CreatedDate = wps.CreatedDate,
                            InsertedBy = wps.InsertedBy,
                            LastUpdated = wps.LastUpdated,
                            UpdatedBy = wps.UpdatedBy
                        }).ToList()
                    }).ToList()
                });
            }

            return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
            {
                Title = "Success",
                Detail = $"Successfully fetched {items.Count} workout(s)",
                Duration = duration,
                IsSuccess = true,
                Data = new Library.Models.PaginationResults.Model<Model>
                {
                    Total = items.Count,
                    Index = processedParams.PageIndex,
                    Size = processedParams.PageSize,
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
                ExerciseId = payload.ExerciseId,
                UserId = payload.UserId,
                WeekPlans = payload.WeekPlans?
                .Select(wp => new Models.Refit.POST.WeekPlan
                {
                    DateOfWeek = wp.DateOfWeek,
                    Time = wp.Time,
                    WeekPlanSets = wp.WeekPlanSets?
                        .Select(wps => new Models.Refit.POST.WeekPlanSet
                        {
                            value = wps.Value
                        })
                        .ToList()
                })
                .ToList()
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
                ExerciseId = payload.ExerciseId,
                UserId = payload.UserId
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
