using System.ComponentModel.DataAnnotations;

namespace Journal.Exercises.Update
{
    public class Payload
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

    }
}
