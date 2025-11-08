using Microsoft.AspNetCore.SignalR;

namespace Journal.Journeys;

public sealed class Hub : Microsoft.AspNetCore.SignalR.Hub
{
    #region [ Fields ]

    private readonly JournalDbContext _context;
    #endregion

    #region [ CTors ]

    public Hub(JournalDbContext context)
    {
        _context = context;
    }
    #endregion
}
