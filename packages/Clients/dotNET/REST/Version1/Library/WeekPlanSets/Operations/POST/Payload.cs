using System.ComponentModel.DataAnnotations;

namespace Library.WeekPlanSets.POST;

public class Payload
{
    [Required]
    public Guid WeekPlanId { get; set; }

    public int Value { get; set; }

}
