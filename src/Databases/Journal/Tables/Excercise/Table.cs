namespace Journal.Databases.Journal.Tables.Excercise
{
    public class Table
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }

    }
}
