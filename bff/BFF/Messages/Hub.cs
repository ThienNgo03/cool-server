using Microsoft.AspNetCore.SignalR;

namespace BFF.Messages;

public sealed class Hub: Microsoft.AspNetCore.SignalR.Hub
{
    private readonly Database.Messages.Context _context;
    public Hub(Database.Messages.Context context)
    {
        _context = context;
    }
    #region [ Overrides ]

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        string? userId = httpContext?.Request.Query["userId"];

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return;

        await Clients.All.SendAsync("ReceiveMessage", $"{userId} ({Context.ConnectionId}) connected");
        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} disconnected");
        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId}: {message}");
    }
}
