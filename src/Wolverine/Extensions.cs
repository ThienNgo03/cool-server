namespace Journal.Wolverine;

public static class Extensions
{
    public static IServiceCollection AddWolverines(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWolverine(options =>
        {
            options.PublishMessage<Journal.Journeys.Delete.Messager.Message>().ToLocalQueue("journey-delete");
            options.PublishMessage<Journal.Journeys.Post.Messager.Message>().ToLocalQueue("journey-post");
            options.PublishMessage<Journal.Journeys.Update.Messager.Message>().ToLocalQueue("journey-update");

            options.PublishMessage<Journal.Users.Delete.Messager.Message>().ToLocalQueue("user-delete");
            options.PublishMessage<Journal.Users.Post.Messager.Message>().ToLocalQueue("user-post");
            options.PublishMessage<Journal.Users.Update.Messager.Message>().ToLocalQueue("user-update");

            options.PublishMessage<Journal.Notes.Delete.Messager.Message>().ToLocalQueue("note-delete");
            options.PublishMessage<Journal.Notes.Post.Messager.Message>().ToLocalQueue("note-post");
            options.PublishMessage<Journal.Notes.Update.Messager.Message>().ToLocalQueue("note-update");

            options.PublishMessage<Journal.Exercises.Delete.Messager.Message>().ToLocalQueue("exercise-delete");
            options.PublishMessage<Journal.Exercises.Post.Messager.Message>().ToLocalQueue("exercise-post");
            options.PublishMessage<Journal.Exercises.Update.Messager.Message>().ToLocalQueue("exercise-update");

            options.PublishMessage<Journal.WorkoutLogs.Delete.Messager.Message>().ToLocalQueue("workoutlog-delete");
            options.PublishMessage<Journal.WorkoutLogs.Post.Messager.Message>().ToLocalQueue("workoutlog-post");
            options.PublishMessage<Journal.WorkoutLogs.Update.Messager.Message>().ToLocalQueue("workoutlog-update");

            options.PublishMessage<Journal.Workouts.Delete.Messager.Message>().ToLocalQueue("workout-delete");
            options.PublishMessage<Journal.Workouts.Post.Messager.Message>().ToLocalQueue("workout-post");
            options.PublishMessage<Journal.Workouts.Update.Messager.Message>().ToLocalQueue("workout-update");

            options.PublishMessage<Journal.WeekPlans.Delete.Messager.Message>().ToLocalQueue("weekplan-delete");
            options.PublishMessage<Journal.WeekPlans.Post.Messager.Message>().ToLocalQueue("weekplan-post");
            options.PublishMessage<Journal.WeekPlans.Update.Messager.Message>().ToLocalQueue("weekplan-update");

            options.PublishMessage<Journal.MeetUps.Delete.Messager.Message>().ToLocalQueue("meetup-delete");
            options.PublishMessage<Journal.MeetUps.Post.Messager.Message>().ToLocalQueue("meetup-post");
            options.PublishMessage<Journal.MeetUps.Update.Messager.Message>().ToLocalQueue("meetup-update");
           
            options.PublishMessage<Journal.Competitions.Delete.Messager.Message>().ToLocalQueue("competition-delete");
            options.PublishMessage<Journal.Competitions.Post.Messager.Message>().ToLocalQueue("competition-post");
            options.PublishMessage<Journal.Competitions.Put.Messager.Message>().ToLocalQueue("competition-update");

            options.PublishMessage<Journal.TeamPools.Delete.Messager.Message>().ToLocalQueue("campaign-delete");
            options.PublishMessage<Journal.TeamPools.Post.Messager.Message>().ToLocalQueue("campaign-post");
            options.PublishMessage<Journal.TeamPools.Put.Messager.Message>().ToLocalQueue("campaign-update");

            options.PublishMessage<Journal.SoloPools.Delete.Messager.Message>().ToLocalQueue("campaign-member-delete");
            options.PublishMessage<Journal.SoloPools.Post.Messager.Message>().ToLocalQueue("campaign-member-post");
            options.PublishMessage<Journal.SoloPools.Put.Messager.Message>().ToLocalQueue("campaign-member-update");

        });
        return services;

    }
}
