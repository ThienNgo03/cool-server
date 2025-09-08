using ExcelDataReader;
using Journal.Databases.Identity;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Journal.Databases;

public static class Extensions
{
    public static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddDbContext<JournalDbContext>(x => x.UseSqlServer("Server=localhost,1433;Database=Journal;User Id=sa;Password=SqlServer2022!;TrustServerCertificate=true;"));
        //services.AddDbContext<IdentityContext>(x => x.UseSqlServer("Server=localhost,1433;Database=IdentityDb;User Id=sa;Password=SqlServer2022!;TrustServerCertificate=true;"));
        services.AddDbContext<JournalDbContext>(x =>
        {
                x.EnableSensitiveDataLogging();
                //x.UseSqlServer("Server=localhost;Database=JournalTest1;Trusted_Connection=True;TrustServerCertificate=True;")
                x.UseSqlServer("Server=localhost;Database=JournalTest;Trusted_Connection=True;TrustServerCertificate=True;")
            .UseSeeding((context, _) =>
            {
                var journalContext = (JournalDbContext)context;
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exercises.xlsx");
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var config = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };

                var rawData = reader.AsDataSet(config);

                // Assuming the first sheet contains your data
                var table = rawData.Tables[0];

                var exercise = table.AsEnumerable().Select(row => new Journal.Tables.Exercise.Table
                {
                    Id = Guid.Parse(row["Id"].ToString()),
                    Name = row["Name"].ToString(),
                    Description = row["Description"].ToString(),
                    Type = row["Type"].ToString(),
                    CreatedDate = DateTime.Now,
                }).ToList();

                journalContext.Exercises.AddRange(exercise);
                journalContext.SaveChanges();
             });
        }
        );
        services.AddDbContext<IdentityContext>(x => x.UseSqlServer("Server=localhost;Database=Identity;Trusted_Connection=True;TrustServerCertificate=True;"));

        services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

        return services;


    }

}
