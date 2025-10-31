namespace Library.WorkoutLogSets;

public interface Interface
{
    Task<Models.PaginationResults.Model<Model>> GetAsync(GET.Parameters parameters);
    Task PostAsync(POST.Payload payload);
    Task PutAsync(PUT.Payload payload);
    Task PatchAsync(Models.Patch.Parameters parameters);
    Task DeleteAsync(DELETE.Parameters parameters);
}

