namespace Journal.Exercises.Get
{
    public class Parameters
    {
        public Guid? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastUpdated { get; set; }

        public int? PageSize { get; set; }

        public int? PageIndex { get; set; }
    }
}
