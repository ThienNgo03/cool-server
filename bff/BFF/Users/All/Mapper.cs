using BFF.Databases.App;
using Bogus;

namespace BFF.Users.All;

public interface IMapper
{
    void SetTime(List<Response> responses);
    void SetImageUrl(List<Response> responses);
    void SetStatus(List<Response> responses);
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
    public void SetTime(List<Response> responses)
    {
        foreach(var response in responses)
        {
            response.Time = TimeSpan.FromHours(faker.Random.Int(0, 23)).Add(TimeSpan.FromMinutes(faker.Random.Int(0, 59)));
        }
    }
    public void SetImageUrl(List<Response> responses)
    {
        foreach (var response in responses)
        {
            var imageId = faker.Random.Number(1, 1000);
            response.ImageUrl = $"https://picsum.photos/id/{imageId}/200/200";
        }
    }
    public void SetStatus(List<Response> responses)
    {
        var statuslist = new[] { "Online", "Offline", "Training" };
        foreach (var response in responses)
        {
            response.Status = faker.PickRandom(statuslist);
        }
    }
}
