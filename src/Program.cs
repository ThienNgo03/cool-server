using Journal.Authentication;
using Journal.Databases;
using Journal.Databases.Identity;
using Journal.Files;
using Journal.Journeys;
using Journal.Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.Configure<DbConfig>(
    builder.Configuration.GetSection("JournalDb"));
builder.Services.Configure<DbConfig>(
    builder.Configuration.GetSection("IdentityDb"));

builder.Services.AddDatabases(builder.Configuration);

builder.Services.AddGrpc();
builder.Services.AddWolverines(builder.Configuration);
builder.Services.AddJourneys(builder.Configuration);
builder.Services.AddSignalR(x => x.EnableDetailedErrors = true);
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddFile(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//app.MapGrpcService<Journal.Beta.Authentication.Login.Service>();
//app.MapGrpcService<Journal.Beta.Authentication.Register.Service>();
app.MapGrpcService<Journal.Notes.Create.Service>();
app.MapGrpcService<Journal.Notes.Search.Service>();

app.MapHub<Journal.Competitions.Hub>("competitions-hub");
app.MapHub<Journal.Exercises.Hub>("exercises-hub");
app.MapHub<Journal.Workouts.Hub>("workouts-hub");
app.MapHub<Journal.WorkoutLogs.Hub>("workout-logs-hub");
app.MapHub<Journal.WeekPlans.Hub>("week-plans-hub");
app.MapHub<Journal.MeetUps.Hub>("meet-ups-hub");
app.MapHub<Journal.WeekPlanSets.Hub>("week-plan-sets-hub");
app.MapHub<Journal.WorkoutLogSets.Hub>("workout-log-sets-hub");
app.MapHub<Journal.Muscles.Hub>("muscles-hub");
app.MapHub<Journal.ExerciseMuscles.Hub>("exercise-muscles-hub");
app.MapHub<Journal.SoloPools.Hub>("solo-pools-hub");
app.MapHub<Journal.TeamPools.Hub>("team-pools-hub");
app.MapHub<Journal.Users.Hub>("users-hub");

app.Run();