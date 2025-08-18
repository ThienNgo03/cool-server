namespace Journal.Databases.Journal.Tables.WeekPlanSet;

public class Table
{
    public Guid Id { get; set; }

    public Guid WeekPlanId { get; set; }

    public int Value { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }
}
