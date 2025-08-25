using System.ComponentModel.DataAnnotations;

namespace Library.WeekPlanSets.Create;

public class Payload
{
    [Required]
    public Guid WeekPlanId { get; set; }

    public int Value { get; set; }

}
