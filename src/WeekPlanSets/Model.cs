namespace Journal.WeekPlanSets;

public class Model : Models.BaseTable.Model
{
    public Guid WeekPlanId { get; set; }
    public int Value { get; set; }
}
