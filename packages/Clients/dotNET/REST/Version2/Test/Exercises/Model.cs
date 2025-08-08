using Newtonsoft.Json;

namespace Test.Exercises;

public class Model
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}
