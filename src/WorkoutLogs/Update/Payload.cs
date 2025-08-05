namespace Journal.WorkoutLogs.Update
{
    public class Payload
    {
        public Guid Id { get; set; }

        public Guid WorkoutId { get; set; }

        public int Rep { get; set; }

        public int HoldingTime { get; set; }

        public int Set { get; set; }

        public DateTime WorkoutDate { get; set; }

    }
}
