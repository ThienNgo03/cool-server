namespace Journal.WorkoutLogs.Get
{
    public class Parameters
    {
        public Guid? Id { get; set; }

        public Guid? WorkoutId { get; set; }

        public DateTime? WorkoutDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastUpdated { get; set; }

        public int? PageSize { get; set; }

        public int? PageIndex { get; set; }
    }
}
