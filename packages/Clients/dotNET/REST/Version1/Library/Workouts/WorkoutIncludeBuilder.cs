using System.Linq.Expressions;
using Library.Queryable.Include;

namespace Library.Workouts;

/// <summary>
/// Concrete implementation of IncludeBuilder specifically for Workout entities
/// Provides fluent Include/ThenInclude syntax with AllAsync execution capability
/// </summary>
/// <typeparam name="TProperty">The current property type in the include chain</typeparam>
internal class WorkoutIncludeBuilder<TProperty> : IncludeBuilder<Model, TProperty>
{
    private readonly Interface _workoutService;

    /// <summary>
    /// Initializes a new WorkoutIncludeBuilder with the workout service
    /// </summary>
    /// <param name="workoutService">The workout service instance for executing queries</param>
    public WorkoutIncludeBuilder(Interface workoutService) : base()
    {
        _workoutService = workoutService;
    }

    /// <summary>
    /// Initializes a new WorkoutIncludeBuilder with existing includes
    /// </summary>
    /// <param name="workoutService">The workout service instance for executing queries</param>
    /// <param name="includes">Existing include strings</param>
    public WorkoutIncludeBuilder(Interface workoutService, IEnumerable<string> includes) : base(includes)
    {
        _workoutService = workoutService;
    }

    /// <summary>
    /// Factory method for creating new builder instances of the same type
    /// </summary>
    protected override IIncludable<Model, TNewProperty> CreateIncludeBuilder<TNewProperty>(List<string> includes)
    {
        return new WorkoutIncludeBuilder<TNewProperty>(_workoutService, includes);
    }

    /// <summary>
    /// Executes the query with all configured includes and returns paginated results
    /// </summary>
    /// <typeparam name="TParameters">The parameter type (should be Workouts.All.Parameters)</typeparam>
    /// <param name="parameters">Parameters for pagination and filtering</param>
    /// <returns>Paginated workout results with included navigation properties</returns>
    public override async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>> AllAsync<TParameters>(TParameters parameters)
    {
        // Cast parameters to workout-specific parameters
        if (parameters is not All.Parameters workoutParams)
        {
            throw new ArgumentException($"Expected {typeof(All.Parameters).Name} but got {typeof(TParameters).Name}", nameof(parameters));
        }

        // Build include string from tracked expressions
        var includeString = GetIncludesString();
        
        if (!string.IsNullOrEmpty(includeString))
        {
            workoutParams.Include = includeString;
        }

        // Execute through the workout service
        return await _workoutService.AllAsync(workoutParams);
    }
}