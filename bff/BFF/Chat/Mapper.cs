namespace BFF.Chat;

public interface IMapper
{
    Messages.IMapper All { get; }
}

public class Mapper : IMapper
{
    public Messages.IMapper All { get; }
    public Mapper(Messages.IMapper all)
    {
        All = all;
    }
}
