using System.ComponentModel.DataAnnotations;

namespace Journal.WeekPlanSets.Update
{
    public class Payload
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid WeekPlanId { get; set; }

        public int Value { get; set; }

    }
}
