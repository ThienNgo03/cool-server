namespace Journal.Users.Get
{
    public class Parameters
    {
        public Guid? id { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? phoneNumber { get; set; }
        public int? pageSize { get; set; }
        public int? pageIndex { get; set; }
    }
}
