namespace Journal.WeekPlanSets.Get;

public class Parameters: Models.PaginationParameters.Model
{

    public Guid? WeekPlanId { get; set; }

    public int? Value { get; set; }

}
