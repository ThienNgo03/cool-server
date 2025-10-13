using BFF.Authentication;
using BFF.Databases;
using BFF.Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR(x => x.EnableDetailedErrors = true);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddWolverine(builder.Configuration);
builder.Services.AddAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapHub<BFF.Chat.Hub>("messages-hub");
app.MapHub<BFF.WorkoutLogTracking.Hub>("workout-log-tracking-hub");
app.MapHub<BFF.Users.Hub>("users-hub");
app.MapHub<BFF.ExerciseConfigurations.Hub>("exercise-configurations-hub");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
