using Wolverine;

namespace BFF.Wolverine;

public static class Extensions
{
    public static IServiceCollection AddWolverine(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWolverine(opts =>
        {
            //Message 
            opts.PublishMessage<Chat.SendMessage.Messager.Message>().ToLocalQueue("message-sent");
            opts.PublishMessage<Chat.DeleteMessage.Messager.Message>().ToLocalQueue("message-deleted");
            opts.PublishMessage<Chat.EditMessage.Messager.Message>().ToLocalQueue("message-edited");
            //Exercise Configurations
            opts.PublishMessage<ExerciseConfigurations.Save.Messager.Message>().ToLocalQueue("saved");
            //Workout Log Tracking
            opts.PublishMessage<WorkoutLogTracking.CreateWorkoutLogs.Messager.Message>().ToLocalQueue("workoutLog-create");
        });
        return services;
    }
}
