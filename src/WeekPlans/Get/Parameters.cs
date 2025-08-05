namespace Journal.WeekPlans.Get
{
    public class Parameters
    {
        public Guid? Id { get; set; }

        public Guid? WorkoutId { get; set; }

        public string? DateOfWeek { get; set; }

        //public DayOfWeek? DateOfWeek {get; set; }

        public DateTime? Time { get; set; }

        public int? Rep { get; set; }

        public TimeSpan? HoldingTime { get; set; }

        public int? Set { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastUpdated { get; set; }

        public int? PageSize { get; set; }

        public int? PageIndex { get; set; }

    }
}
