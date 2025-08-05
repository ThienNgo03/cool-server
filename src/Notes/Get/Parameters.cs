using Microsoft.AspNetCore.Mvc;

namespace Journal.Notes.Get
{
    public class Parameters
    {
        public Guid? id { get; set; }
        public Guid? journeyId { get; set; }
        public Guid? userId { get; set; }
        public string? content { get; set; }
        public string? mood { get; set; }
        public DateTime? date { get; set; }
        public int? pageSize { get; set; }
        public int? pageIndex { get; set; }
    }
}
