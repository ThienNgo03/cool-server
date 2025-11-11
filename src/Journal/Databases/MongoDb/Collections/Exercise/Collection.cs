namespace Journal.Databases.MongoDb.Collections.Exercise;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Collection
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime? LastUpdated { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("muscles")]
    public List<Muscle>? Muscles { get; set; }
}

public class Muscle
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime? LastUpdated { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
}
