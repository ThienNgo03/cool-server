using MongoDB.Bson.Serialization.Attributes;

namespace Journal.Databases.MongoDb.Collections.Workout;

public class Collection
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("exerciseId")]
    public Guid ExerciseId { get; set; }

    [BsonElement("userId")]
    public Guid UserId { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [BsonElement("exercise")]
    public Exercise? Exercise { get; set; }

    [BsonElement("weekPlans")]
    public List<WeekPlan>? WeekPlans { get; set; } = new();
}

public class Exercise
{
    [BsonElement("id")]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }

    [BsonElement("type")]
    public string Type { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [BsonElement("muscles")]
    public List<Muscle>? Muscles { get; set; } = new();
}

public class Muscle
{
    [BsonElement("id")]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}

public class WeekPlan
{
    [BsonElement("id")]
    public Guid Id { get; set; }

    [BsonElement("workoutId")]
    public Guid WorkoutId { get; set; }

    [BsonElement("dateOfWeek")]
    public string DateOfWeek { get; set; }

    [BsonElement("time")]
    public TimeSpan Time { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [BsonElement("weekPlanSets")]
    public List<WeekPlanSet>? WeekPlanSets { get; set; } = new();
}

public class WeekPlanSet
{
    [BsonElement("id")]
    public Guid Id { get; set; }

    [BsonElement("weekPlanId")]
    public Guid WeekPlanId { get; set; }

    [BsonElement("value")]
    public int Value { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [BsonElement("insertedBy")]
    public Guid CreatedById { get; set; }

    [BsonElement("updatedBy")]
    public Guid UpdatedById { get; set; }
}