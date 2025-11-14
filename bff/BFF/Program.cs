using BFF.Authentication;
using BFF.Chat;
using BFF.Databases;
using BFF.Exercises;
using BFF.Exercises.Configurations;
using BFF.Users;
using BFF.Wolverine;
using Library;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddExercises();
builder.Services.AddExerciseConfigurations();
builder.Services.AddUsers();
//builder.Services.AddChat();
builder.Services.AddSignalR(x => x.EnableDetailedErrors = true);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddWolverine(builder.Configuration);
builder.Services.AddAuthentication(builder.Configuration);

Library.Config locaHostConfig = new("http://localhost:6000/");
Library.Config devTunnelEnviroment = new("https://k7ql47j3-7011.asse.devtunnels.ms/");
Library.Config productionEnviroment = new("https://storm-ergshka6h7a0bngn.southeastasia-01.azurewebsites.net/");
builder.Services.AddEndpoints(locaHostConfig);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapHub<BFF.Chat.Hub>("messages-hub");
app.MapHub<BFF.WorkoutLogTracking.Hub>("workout-log-tracking-hub");
app.MapHub<BFF.Users.Hub>("users-hub");
app.MapHub<BFF.Exercises.Configurations.Hub>("exercise-configurations-hub");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
