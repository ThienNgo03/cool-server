using Journal.Databases;
using Journal.Wolverine;
using Journal.Journeys;
using Journal.Authentication;
using Journal.Files;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();

//đăng ký service 
builder.Services.AddDatabases(builder.Configuration);
builder.Services.AddWolverines(builder.Configuration);
builder.Services.AddJourneys(builder.Configuration);
builder.Services.AddSignalR(x => x.EnableDetailedErrors = true);
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddFile(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

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

app.MapHub<Journal.SoloPools.Hub> ("solo-pools-hub");

app.MapHub<Journal.TeamPools.Hub>("team-pools-hub");

app.MapHub<Journal.Users.Hub>("users-hub");


app.Run();
