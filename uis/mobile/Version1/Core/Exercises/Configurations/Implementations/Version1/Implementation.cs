using Core.Exercises.Configurations;
using Core.Exercises.Configurations.Detail;
using Core.Exercises.Configurations.Save;
using System.Diagnostics;

namespace Core.Exercises.Configurations.Implementations.Version1;

public class Implementation: Interface
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

    public async Task<Response> DetailAsync(Parameters parameters)
    {
        var refitParameters = new Models.Refit.Detail.Parameters
        {
            UserId = parameters.UserId
        };
        try
        {
            var response = await refitInterface.Detail(refitParameters, parameters.ExerciseId.Value);

            if (response is null || response.Content is null)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return new Response
            {
                WorkoutId = response.Content.WorkoutId,
                UserId = response.Content.UserId,
                WeekPlans = response.Content.WeekPlans?.Select(wp => new Detail.WeekPlan
                {
                    DateOfWeek = wp.DateOfWeek,
                    Time = wp.Time,
                    WeekPlanSets = wp.WeekPlanSets?.Select(wps => new Detail.WeekPlanSet
                    {
                        Id = wps.Id,
                        Value = wps.Value
                    }).ToList()
                }).ToList()
            };

        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task SaveAsync(Payload payload)
    {
        try
        {
            var refitPayload = new Models.Refit.Save.Payload
            {
                ExerciseId = payload.ExerciseId,
                UserId = payload.UserId,
                WeekPlans = payload.WeekPlans?.Select(wp => new Models.Refit.Save.WeekPlan
                {
                    DateOfWeek = wp.DateOfWeek,
                    Time = wp.Time,
                    WeekPlanSets = wp.WeekPlanSets?.Select(wps => new Models.Refit.Save.WeekPlanSet
                    {
                        Value = wps.Value
                    }).ToList()
                }).ToList()
            };
            var response = await refitInterface.Save(refitPayload, payload.ExerciseId);
        }
        catch (Exception ex)
        {
            throw new Exception();
        }
    }
}
