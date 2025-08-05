namespace Journal.Databases.Journal.Tables.WorkoutLog
{
    public class Table
    {
        public Guid Id { get; set; }

        public Guid WorkoutId { get; set; }

        public int Rep { get; set; } // làm sao để chỉ có một trong hai Rep hoặc HoldingTime

        public int HoldingTime { get; set; }

        public int Set { get; set; }

        public DateTime WorkoutDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
