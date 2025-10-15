using Wolverine;

namespace BFF.Wolverine;

public static class Extensions
{
    public static IServiceCollection AddWolverine(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWolverine(opts =>
        {
            //Message 
            opts.PublishMessage<Chat.Send.Messager.Message>().ToLocalQueue("message-create");
            //Exercise Configurations
            opts.PublishMessage<ExerciseConfigurations.Save.Messager.Message>().ToLocalQueue("saved");
            //Workout Log Tracking
            opts.PublishMessage<WorkoutLogTracking.CreateWorkoutLogs.Messager.Message>().ToLocalQueue("workoutLog-create");
        });
        return services;
    }
}
