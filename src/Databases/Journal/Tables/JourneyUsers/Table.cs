namespace Journal.Databases.Journal.Tables.JourneyUsers
{
    public class Table
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid JourneyId { get; set; }
    }
}
