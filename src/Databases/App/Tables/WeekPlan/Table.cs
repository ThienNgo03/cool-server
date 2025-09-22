namespace Journal.Databases.App.Tables.WeekPlan;
public class Table
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public string DateOfWeek { get; set; }

    public TimeSpan Time { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}

public enum DateOfWeek
{
    Monday , Tuesday, Wednesday, Thursday , Friday, Saturday, Sunday
}
