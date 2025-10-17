using Bogus;

namespace BFF.Chat.Messages;

public interface IMapper
{
    void SetAvatar(List<Response> responses);
}
public class Mapper:IMapper
{
    private readonly Faker faker;
    private readonly Databases.Messages.Context context;
    public Mapper(Databases.Messages.Context context)
    {
        this.context = context;
        this.faker = new Faker();
    }
    public void SetAvatar(List<Response> responses)
    {
        foreach (var response in responses)
        {
            var imageId = faker.Random.Number(1, 1000);
            response.Avatar = $"https://picsum.photos/id/{imageId}/200/200";
        }
    }
}
