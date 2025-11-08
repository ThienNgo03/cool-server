using BFF.Databases.App;
using Bogus;

namespace BFF.Exercises.All;

public interface IMapper
{
    void AttachImageUrls(List<Item> responses);

    void SetSubTitle(List<Item> responses);

    void SetBadge(List<Item> responses);

    void SetPercentageCompletion(List<Item> responses);

    void SetPercentageCompletionString(List<Item> responses);

    void SetBadgeTextColor(List<Item> responses);

    void SetBadgeBackgroundColor(List<Item> responses);
}

public class Mapper : IMapper
{
    private readonly Faker faker;
    private readonly JournalDbContext context;
    public Mapper(JournalDbContext context)
    {
        this.context = context;
        this.faker = new Faker();
    }

    public void SetSubTitle(List<Item> responses)
    {
        var muscleGroups = context.Muscles.ToDictionary(m => m.Id, m => m.Name);
        var exerciseMuscles = context.ExerciseMuscles
            .GroupBy(em => em.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Select(em => em.MuscleId).ToList());

        foreach (var response in responses)
        {
            if (exerciseMuscles.TryGetValue(response.Id, out var muscleIds))
            {
                var muscleNames = muscleIds
                    .Where(id => muscleGroups.ContainsKey(id))
                    .Select(id => muscleGroups[id]);

                response.SubTitle = string.Join(", ", muscleNames);
            }
            else
            {
                response.SubTitle = string.Empty;
            }
        }
    }

    public void AttachImageUrls(List<Item> responses)
    {
        foreach (var response in responses)
        {
            var imageId = faker.Random.Number(1, 1000);
            response.ImageUrl = $"https://picsum.photos/id/{imageId}/200/200";
        }
    }

    public void SetBadge(List<Item> responses)
    {
        //use faker generate badge with in range Easy, Medium, Hard
        var badges = new[] { "Easy", "Medium", "Hard" };
        foreach (var response in responses)
        {
            response.Badge = faker.PickRandom(badges);
        }
    }


    public void SetBadgeTextColor(List<Item> responses)
    {
        foreach (var response in responses)
        {
            response.BadgeTextColor = response.Badge switch
            {
                "Easy" => "#2E7D32", // green
                "Medium" => "#F9A825", // yellow
                "Hard" => "#C62828", // red
                _ => "#000000" // default to black
            };
        }
    }

    public void SetBadgeBackgroundColor(List<Item> responses)
    {
        foreach (var response in responses)
        {
            response.BadgeBackgroundColor = response.Badge switch
            {
                "Easy" => "#DFF5E1", // green
                "Medium" => "#FFF4CC", // yellow
                "Hard" => "#FDE0E0", // red
                _ => "#000000" // default to black
            };
        }
    }

    public void SetPercentageCompletion(List<Item> responses)
    {
        //use faker to generate percentage between 0 and 100
        foreach (var response in responses)
        {
            response.PercentageComplete = Math.Round(faker.Random.Double(0, 100), 2);
        }
    }

    public void SetPercentageCompletionString(List<Item> responses)
    {
        //get the percentage complete from each response and convert to string with % sign
        foreach (var response in responses)
        {
            response.PercentageCompleteString = $"{response.PercentageComplete}%";
        }
    }

}