namespace Library.WeekPlans.Update
{
    public class Payload
    {
        public Guid Id { get; set; }

        public Guid WorkoutId { get; set; }

        public string DateOfWeek { get; set; }

        public DateTime Time { get; set; }
    }
}
