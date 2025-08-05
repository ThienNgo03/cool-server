namespace Journal.Notes.Post
{
    public class Payload
    {
        public Guid UserId { get; set; }
        public Guid JourneyId { get; set; }
        public string Content { get; set; }
        public string Mood { get; set; }
        public DateTime Date { get; set; }
    }
}
