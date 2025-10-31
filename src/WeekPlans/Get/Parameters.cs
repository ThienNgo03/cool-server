namespace Journal.WeekPlans.Get
{
    public class Parameters
    {
        public string? Ids { get; set; }

        public Guid? WorkoutId { get; set; }

        public string? DateOfWeek { get; set; }

        public TimeSpan? Time { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastUpdated { get; set; }

        public int? PageSize { get; set; }

        public int? PageIndex { get; set; }

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }
    }
}
