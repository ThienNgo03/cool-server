using Newtonsoft.Json;

namespace Test.Competitions;

public class Model
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("participantIds")]
    public List<Guid> ParticipantIds { get; set; } = [];

    [JsonProperty("exerciseId")]
    public Guid ExerciseId { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; } = string.Empty;

    [JsonProperty("dateTime")]
    public DateTime DateTime { get; set; }

    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}
