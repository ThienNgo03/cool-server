using Cassandra.Data.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Test.ExerciseMuscles;

public class Test:BaseTest
{
    #region [ CTors ]
    public Test() : base() { }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET_DataExist()
    {
        var dbContext = serviceProvider!.GetRequiredService<Databases.CassandraCql.Context>();
        var exerciseId = "5B4E7C2A-9D3F-4E1B-A6C8-1F2A3D7C9E5B";
        var muscleId = "E22E658A-A4B6-4B86-98A6-7F98563DF70A";
        var exerciseMuscle = new Databases.CassandraCql.Tables.ExerciseMuscle.Table()
        {
            Id = Guid.NewGuid(),
            ExerciseId = Guid.Parse(exerciseId),
            MuscleId = Guid.Parse(muscleId),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        await dbContext.ExerciseMuscles.Insert(exerciseMuscle).ExecuteAsync();
        var exerciseMusclesEndpoint = serviceProvider!.GetRequiredService<Library.ExerciseMuscles.Interface>();
        var result = await exerciseMusclesEndpoint.GetAsync(new()
        {
            PartitionKey= exerciseMuscle.MuscleId,
        });
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count > 0, "Expected at least one exercise muscle in result.");
        Assert.True(result.Items.Any(e => e.MuscleId == exerciseMuscle.MuscleId), "Expected to find the added exercise muscle in the result.");
        await dbContext.ExerciseMuscles
            .Where(x => x.MuscleId == exerciseMuscle.MuscleId && x.Id==exerciseMuscle.Id)
            .Delete()
            .ExecuteAsync();
    }

    [Fact]
    public async Task POST()
    {
        var dbContext = serviceProvider!.GetRequiredService<Databases.CassandraCql.Context>();
        var exerciseId =Guid.Parse("5B4E7C2A-9D3F-4E1B-A6C8-1F2A3D7C9E5B") ;
        var muscleId =Guid.Parse("E22E658A-A4B6-4B86-98A6-7F98563DF70A") ;
        var exerciseMuscle = new Library.ExerciseMuscles.POST.Payload()
        {
            ExerciseId = exerciseId,
            MuscleId = muscleId,
        };
        var exerciseMusclesEndpoint = serviceProvider!.GetRequiredService<Library.ExerciseMuscles.Interface>();
        await exerciseMusclesEndpoint.CreateAsync(exerciseMuscle);
        var result=await dbContext.ExerciseMuscles.Where(x => x.MuscleId == muscleId).FirstOrDefault().ExecuteAsync();
        Assert.NotNull(result);
        await dbContext.ExerciseMuscles
            .Where(x => x.MuscleId == exerciseMuscle.MuscleId&&x.Id==result.Id)
            .Delete()
            .ExecuteAsync();
    }

    [Fact]
    public async Task PUT()
    {
        var dbContext = serviceProvider!.GetRequiredService<Databases.CassandraCql.Context>();
        var exerciseId = Guid.Parse("5B4E7C2A-9D3F-4E1B-A6C8-1F2A3D7C9E5B");
        var muscleId = Guid.Parse("E22E658A-A4B6-4B86-98A6-7F98563DF70A");
        var exerciseMuscle = new Databases.CassandraCql.Tables.ExerciseMuscle.Table()
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            MuscleId = muscleId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        await dbContext.ExerciseMuscles.Insert(exerciseMuscle).ExecuteAsync();

        var newExerciseId = Guid.Parse("6b8c1f2e-9d3a-4a6e-b5c8-1f2d3e7c9a4b");
        var newMuscleId = Guid.Parse("cac3845c-7354-47f3-9f40-e7c6cefde8a8");
        var exerciseMusclesEndpoint = serviceProvider!.GetRequiredService<Library.ExerciseMuscles.Interface>();
        var updatedExerciseMuscle = new Library.ExerciseMuscles.PUT.Payload()
        {
            PartitionKey=muscleId,
            Id = exerciseMuscle.Id,
            NewExerciseId = newExerciseId,
            NewMuscleId = newMuscleId,
        };
        await exerciseMusclesEndpoint.UpdateAsync(updatedExerciseMuscle);
        var result = await dbContext.ExerciseMuscles.Where(x => x.MuscleId == newMuscleId).FirstOrDefault().ExecuteAsync();
        Assert.NotNull(result);
        await dbContext.ExerciseMuscles
            .Where(x => x.MuscleId == newMuscleId && x.Id == result.Id)
            .Delete()
            .ExecuteAsync();
    }

    [Fact]
    public async Task DELETE()
    {
        var dbContext = serviceProvider!.GetRequiredService<Databases.CassandraCql.Context>();
        var exerciseId = "5B4E7C2A-9D3F-4E1B-A6C8-1F2A3D7C9E5B";
        var muscleId = "E22E658A-A4B6-4B86-98A6-7F98563DF70A";
        var exerciseMuscle = new Databases.CassandraCql.Tables.ExerciseMuscle.Table()
        {
            Id = Guid.NewGuid(),
            ExerciseId = Guid.Parse(exerciseId),
            MuscleId = Guid.Parse(muscleId),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        await dbContext.ExerciseMuscles.Insert(exerciseMuscle).ExecuteAsync();
        var exerciseMusclesEndpoint = serviceProvider!.GetRequiredService<Library.ExerciseMuscles.Interface>();
        await exerciseMusclesEndpoint.DeleteAsync(new()
        {
            PartitionKey = exerciseMuscle.MuscleId,
            Id = exerciseMuscle.Id
        });
        var result = await dbContext.ExerciseMuscles
            .Where(x => x.MuscleId == exerciseMuscle.MuscleId && x.Id==exerciseMuscle.Id)
            .FirstOrDefault()
            .ExecuteAsync();
        Assert.Null(result);
    }

    #endregion
}
