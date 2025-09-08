using ExcelDataReader;
using System.Data;

namespace Journal.Databases.Journal;

public class ImportExcel
{
    public async Task<List<Tables.Exercise.Table>> ImportExercise()
    {

        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Calisthenic.xlsx");
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

        return table.AsEnumerable().Select(row => new Tables.Exercise.Table
        {
            Id = Guid.Parse(row["Id"].ToString()),
            Name = row["Name"].ToString(),
            Description = row["Description"].ToString(),
            Type = row["Type"].ToString(),
            CreatedDate = DateTime.Parse(row["CreatedDate"].ToString()),
            LastUpdated = DateTime.Parse(row["UpdatedDate"].ToString())
        }).ToList();
    }
}
