namespace Journal.Notes.Update
{
    public class Payload
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid JourneyId { get; set; }
        public string Content { get; set; }
        public string Mood { get; set; }
        public DateTime Date { get; set; }
    }
}
