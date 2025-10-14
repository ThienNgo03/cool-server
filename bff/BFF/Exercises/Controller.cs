using BFF.Databases.App;
using BFF.Exercises.GetExercises;
using BFF.Models.PaginationResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BFF.Exercises;

[Route("api/exercises")]
[ApiController]
public class Controller : ControllerBase
{
    private readonly JournalDbContext _context;
    public Controller(JournalDbContext context)
    {
        _context = context;
    }

    [HttpGet("get-exercises")]
    public async Task<IActionResult> GetExercises([FromQuery] GetExercises.Parameters parameters)
    {
        var query = _context.Exercises.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Ids))
        {
            var ids = parameters.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                        .Where(guid => guid.HasValue)
                        .Select(guid => guid.Value)
                        .ToList();
            query = query.Where(x => ids.Contains(x.Id));
        }

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(x => x.Name.Contains(parameters.Name));

        if (!string.IsNullOrEmpty(parameters.Description))
            query = query.Where(x => x.Description.Contains(parameters.Description));

        if (!string.IsNullOrEmpty(parameters.Type))
            query = query.Where(x => x.Type.Contains(parameters.Type));

        if (parameters.CreatedDate.HasValue)
            query = query.Where(x => x.CreatedDate == parameters.CreatedDate);

        if (parameters.LastUpdated.HasValue)
            query = query.Where(x => x.LastUpdated == parameters.LastUpdated);

        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
            query = query.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);

        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            var sortBy = typeof(Databases.App.Tables.Exercise.Table)
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals(parameters.SortBy, StringComparison.OrdinalIgnoreCase))
                ?.Name;
            if (sortBy != null)
            {
                query = parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(x => EF.Property<object>(x, sortBy))
                    : query.OrderBy(x => EF.Property<object>(x, sortBy));
            }
        }

        var result = await query.AsNoTracking().ToListAsync();
        var exerciseIds = result.Select(x => x.Id).ToList();

        var responses = result.Select(exercise => new GetExercises.Response
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Type = exercise.Type,
            CreatedDate = exercise.CreatedDate,
            LastUpdated = exercise.LastUpdated
        }).ToList();

        var paginationResults = new Builder<GetExercises.Response>()
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(responses.Count)
            .WithItems(responses)
            .Build();

        if (string.IsNullOrEmpty(parameters.Include))
        {
            return Ok(paginationResults);
        }

        var includes = parameters.Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(i => i.Trim().ToLower())
                             .ToList();

        if (!includes.Any(inc => inc.Split(".")[0] == "muscles") || !exerciseIds.Any())
        {
            return Ok(paginationResults);
        }

        var exerciseMusclesTask = _context.ExerciseMuscles
            .Where(x => exerciseIds.Contains(x.ExerciseId))
            .ToListAsync();

        var exerciseMuscles = await exerciseMusclesTask;
        var muscleIds = exerciseMuscles.Select(x => x.MuscleId).Distinct().ToList();

        if (!muscleIds.Any())
        {
            return Ok(paginationResults);
        }

        var muscles = await _context.Muscles
            .Where(x => muscleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        var exerciseMuscleGroups = exerciseMuscles
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Select(em => em.MuscleId));

        foreach (var response in responses)
        {
            if (!exerciseMuscleGroups.TryGetValue(response.Id, out var responseMuscleIds))
            {
                continue;
            }

            response.Muscles = responseMuscleIds
                .Where(muscleId => muscles.ContainsKey(muscleId))
                .Select(muscleId => new GetExercises.Muscle
                {
                    Id = muscles[muscleId].Id,
                    Name = muscles[muscleId].Name,
                    CreatedDate = muscles[muscleId].CreatedDate,
                    LastUpdated = muscles[muscleId].LastUpdated
                })
                .ToList();
        }

        if (string.IsNullOrEmpty(parameters.MusclesSortBy))
            return Ok(paginationResults);

        var normalizeProp = typeof(Databases.App.Tables.Muscle.Table)
            .GetProperties()
            .FirstOrDefault(p => p.Name.Equals(parameters.MusclesSortBy, StringComparison.OrdinalIgnoreCase))
            ?.Name;

        if (normalizeProp == null)
            return Ok(paginationResults);

        var prop = typeof(Databases.App.Tables.Muscle.Table).GetProperty(normalizeProp);
        if (prop == null)
            return Ok(paginationResults);

        var isDescending = parameters.MusclesSortOrder?.ToLower() == "desc";
        foreach (var response in responses.Where(r => r.Muscles?.Any() == true))
        {
            if (response.Muscles == null || !response.Muscles.Any())
                continue;
            response.Muscles = isDescending
                ? response.Muscles.OrderByDescending(m => prop.GetValue(m)).ToList()
                : response.Muscles.OrderBy(m => prop.GetValue(m)).ToList();
        }

        return Ok(paginationResults);
    }

    
}
