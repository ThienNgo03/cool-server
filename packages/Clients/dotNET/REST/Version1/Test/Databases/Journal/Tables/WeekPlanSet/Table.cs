namespace Test.Databases.Journal.Tables.WeekPlanSet;

public class Table
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid InsertedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid UpdatedBy { get; set; }
    public Guid WeekPlanId { get; set; }
    public int Value { get; set; }
}
