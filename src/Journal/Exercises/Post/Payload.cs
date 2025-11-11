using System.ComponentModel.DataAnnotations;

namespace Journal.Exercises.Post
{
    public class Payload
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

    }
}
