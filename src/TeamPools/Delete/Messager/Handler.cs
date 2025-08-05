namespace Journal.TeamPools.Delete.Messager;

public class Handler
{
    private readonly JournalDbContext _context;
    public Handler(JournalDbContext context)
    {
        _context = context;
    }
    public async Task Handle(Message message)
    {
        
    }
}
