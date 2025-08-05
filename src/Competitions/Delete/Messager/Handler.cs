namespace Journal.Competitions.Delete.Messager;

public class Handler
{
    private readonly JournalDbContext _context;
    public Handler(JournalDbContext context)
    {
        _context = context;
    }
    public async Task Handle(Message message)
    {
        if (message.DeleteSoloPool)
        {
            var soloPool = await _context.SoloPools.Where(sp => sp.CompetitionId == message.Id).ToListAsync();
            if (soloPool.Count != 0)
            {
                await _context.SoloPools.Where(sp => sp.CompetitionId == message.Id).ExecuteDeleteAsync();
            }
        }
        if (message.DeleteTeamPool)
        {
            var teamPool = await _context.TeamPools.Where(sp => sp.CompetitionId == message.Id).ToListAsync();
            if (teamPool.Count != 0)
            {
                await _context.TeamPools.Where(sp => sp.CompetitionId == message.Id).ExecuteDeleteAsync();
            }
        }
        await _context.SaveChangesAsync();
    }
}
