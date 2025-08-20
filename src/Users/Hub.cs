using Microsoft.AspNetCore.SignalR;

namespace Journal.Users;

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

    #region [Methods]
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId}: {message}");
    }
    #endregion
}
