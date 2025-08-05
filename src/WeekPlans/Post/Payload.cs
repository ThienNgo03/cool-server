namespace Journal.WeekPlans.Post
{
    public class Payload
    {
        public Guid WorkoutId { get; set; }

        public string DateOfWeek { get; set; }

        //public DayOfWeek DateOfWeek { get; set; }

        public DateTime Time { get; set; }

        public int Rep { get; set; }

        public TimeSpan HoldingTime { get; set; }

        public int Set { get; set; }

    }
}
