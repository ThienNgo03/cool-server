using System.ComponentModel.DataAnnotations;

namespace Library.WeekPlanSets.Implementations.Version1.Models.Refit.POST;

public class Payload
{
    [Required]
    public Guid WeekPlanId { get; set; }

    public int Value { get; set; }

}
