using Library.Queryable;

namespace Library.Workouts;

public interface Interface : IResourceQueryable<Model>
{
    Task CreateAsync(Create.Payload payload);
    Task UpdateAsync(Update.Payload payload);
    Task DeleteAsync(Delete.Parameters parameters);
}
