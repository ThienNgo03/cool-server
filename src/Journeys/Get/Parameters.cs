using Microsoft.AspNetCore.Mvc;

namespace Journal.Journeys.Get
{
    public class Parameters
    {
        public Guid? id { get; set; }
        public string? content { get; set; }
        public string? location { get; set; }
        public DateTime? date { get; set; }
        public int? pageSize { get; set; }
        public int? pageIndex { get; set; }
    }
}
