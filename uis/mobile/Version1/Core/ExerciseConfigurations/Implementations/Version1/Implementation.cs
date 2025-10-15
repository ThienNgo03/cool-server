using Core.ExerciseConfigurations.Detail;
using Core.ExerciseConfigurations.Save;
using System.Diagnostics;

namespace Core.ExerciseConfigurations.Implementations.Version1;

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

    public async Task<Detail.Response> DetailAsync(Parameters parameters)
    {
        var refitParameters = new Models.Refit.Detail.Parameters
        {
            ExerciseId = parameters.ExerciseId,
            UserId = parameters.UserId
        };
        try
        {
            var response = await this.refitInterface.Detail(refitParameters);

            if (response is null || response.Content is null)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return new Detail.Response
            {
                WorkoutId = response.Content.WorkoutId,
                UserId = response.Content.UserId,
                PercentageCompletion = response.Content.PercentageCompletion,
                Difficulty = response.Content.Difficulty,
                Exercise = new Detail.Exercise
                {
                    Id = response.Content.Exercise.Id,
                    Name = response.Content.Exercise.Name,
                    Muscles = response.Content.Exercise.Muscles?.Select(m => new Detail.Muscle
                    {
                        Name = m.Name
                    }).ToList()
                },
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
            throw new NotImplementedException();
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
            var response = await this.refitInterface.Save(refitPayload);
        }
        catch (Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
