using System.ComponentModel.DataAnnotations;

namespace Library.WeekPlanSets.Implementations.Version1.Models.Refit.PUT;

public class Payload
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid WeekPlanId { get; set; }

    public int Value { get; set; }

}
