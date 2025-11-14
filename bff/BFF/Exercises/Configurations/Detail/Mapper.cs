using Bogus;

namespace BFF.Exercises.Configurations.Detail;


public interface IMapper
{
    string PercentageCompletion();

    string Difficulty();
}

public class Mapper : IMapper
{
    private readonly Faker faker;
    public Mapper()
    {
        this.faker = new Faker();
    }

    public string PercentageCompletion()
    {
        var percentage = Math.Round(faker.Random.Double(0, 100), 2);
        return $"{percentage}%";
    }

    public string Difficulty()
    {
        var difficulties = new[] { "Easy", "Medium", "Hard" };
        return faker.PickRandom(difficulties);
    }
}
