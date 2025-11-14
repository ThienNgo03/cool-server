namespace BFF.Exercises.Configurations;

public interface IMapper
{
    Detail.IMapper Detail { get; }
}

public class Mapper : IMapper
{
    public Detail.IMapper Detail { get; }
    public Mapper(Detail.IMapper detail)
    {
        Detail = detail;
    }
} 
