using BFF.Databases.App;
using Bogus;

namespace BFF.Users.All;

public interface IMapper
{
    TimeSpan SetTime();
    string SetImageUrl();
    string SetStatus();
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
    public TimeSpan SetTime()
    {
        TimeSpan time = TimeSpan.FromHours(faker.Random.Int(0, 23)).Add(TimeSpan.FromMinutes(faker.Random.Int(0, 59)));
        return time;
    }
    public string SetImageUrl()
    {
        var imageId = faker.Random.Number(1, 1000);
        string ImageUrl = $"https://picsum.photos/id/{imageId}/200/200";
        return ImageUrl;
    }
    public string SetStatus()
    {
        var statuslist = new[] { "Online", "Offline", "Training" };
        
        string status = faker.PickRandom(statuslist);
        return status;
    }
}
