using Library.Context;
using Library.Sets;

namespace Test;

public class JournalContext : ApiContext
{
    public JournalContext(ApiContextOptions options) : base(options)    
    {
        
    }

    public JournalContext(ApiContextOptions options, HttpClient httpClient) : base(options, httpClient)
    {
        
    }

    public IApiSet<Exercises.Model> Exercises { get; private set; } = null!;
    public IApiSet<Competitions.Model> Competitions { get; private set; } = null!;

    protected override void OnEndpointRegistering()
    {
        Exercises = RegisterEndpoint<Exercises.Model>()
            .WithEndpoint("/exercises")
            .Build();

        Competitions = RegisterEndpoint<Competitions.Model>()
            .WithEndpoint("/competitions")
            .Build();
    }
}
