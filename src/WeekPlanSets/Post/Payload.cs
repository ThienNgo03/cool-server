using System.ComponentModel.DataAnnotations;

namespace Journal.WeekPlanSets.Post
{
    public class Payload
    {
        [Required]
        public Guid WeekPlanId { get; set; }

        public int Value { get; set; }

    }
}
