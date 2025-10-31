using ExcelDataReader;
using System.Data;
namespace Journal.Databases.App;

public class SeedFactory
{
    public async Task SeedExercise(JournalDbContext context)
    {

        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Databases/App/Tables/Exercise/Exercises.xlsx");
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var config = new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        };

        var result = reader.AsDataSet(config);

        // Assuming the first sheet contains your data
        var table = result.Tables[0];

        var exercise = table.AsEnumerable()
                        .Where(x => x.Field<string>("Id") != null &&
                                    x.Field<string>("Name") != null &&
                                    x.Field<string>("Description") != null &&
                                    x.Field<string>("Type") != null)
                        .Select(row => new Exercises.Table
                        {
                            Id = Guid.Parse(row["Id"].ToString()!),
                            Name = row["Name"].ToString()!,
                            Description = row["Description"].ToString()!,
                            Type = row["Type"].ToString()!,
                            CreatedDate = DateTime.Now,
                        }).ToList();
        context.Exercises.AddRange(exercise);
        await context.SaveChangesAsync();
    }
    public async Task SeedMuscle(JournalDbContext context)
    {

        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Databases/App/Tables/Muscle/Muscle.xlsx");
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var config = new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        };

        var result = reader.AsDataSet(config);

        // Assuming the first sheet contains your data
        var table = result.Tables[0];

        var muscle = table.AsEnumerable()
                        .Where(x => x.Field<string>("Id") != null &&
                                    x.Field<string>("Name") != null)
                        .Select(row => new Muscles.Table
                        {
                            Id = Guid.Parse(row["Id"].ToString()!),
                            Name = row["Name"].ToString()!,
                            CreatedDate = DateTime.Now,
                        }).ToList();
        context.Muscles.AddRange(muscle);
        await context.SaveChangesAsync();
    }
    public async Task SeedExerciseMuscle(JournalDbContext context)
    {

        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Databases/App/Tables/ExerciseMuscle/ExerciseMuscle.xlsx");
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var config = new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        };

        var result = reader.AsDataSet(config);

        // Assuming the first sheet contains your data
        var table = result.Tables[0];

        var exerciseMuscle = table.AsEnumerable()
                        .Where(x => x.Field<string>("Id") != null &&
                                    x.Field<string>("ExerciseId") != null&&
                                    x.Field<string>("MuscleId") != null)
                        .Select(row => new ExerciseMuscles.Table
                        {
                            Id = Guid.Parse(row["Id"].ToString()!),
                            ExerciseId = Guid.Parse(row["ExerciseId"].ToString()!),
                            MuscleId = Guid.Parse(row["MuscleId"].ToString()!),
                            CreatedDate = DateTime.Now,
                        }).ToList();
        context.ExerciseMuscles.AddRange(exerciseMuscle);
        await context.SaveChangesAsync();
    }

    public async Task SeedAdmins(JournalDbContext context)
    {
        var id = Guid.Parse("fdfa4136-ada3-41dc-b16e-8fd9556d4574");
        var testAdmin = new Users.Table
        {
            Id = id,
            Name = "systemtester",
            Email = "systemtester@journal.com",
            PhoneNumber = "0564330462",
            ProfilePicture = null,
            CreatedDate = DateTime.UtcNow
        };
        context.Users.Add(testAdmin);
        await context.SaveChangesAsync();
    }
}
