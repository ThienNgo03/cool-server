namespace Journal.WorkoutLogs.Get
{
    public class Parameters: Models.PaginationParameters.Model
    {
        public Guid? WorkoutId { get; set; }

        public DateTime? WorkoutDate { get; set; }
    }
}
