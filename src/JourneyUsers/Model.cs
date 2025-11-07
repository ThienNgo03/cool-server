namespace Journal.JourneyUsers;

public class Model : Models.Base
{
    public Guid UserId { get; set; }

    public Guid JourneyId { get; set; }
}
