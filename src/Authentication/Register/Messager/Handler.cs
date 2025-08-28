namespace Journal.Authentication.Register.Messager;

public class Handler
{
    private readonly JournalDbContext _context;
    public Handler(JournalDbContext context)
    {
        _context = context;
    }
    public async Task Handle(Message message)
    {
        var newAccount = new Databases.Journal.Tables.User.Table
        {
            Id = message.id,
            Name = message.Payload.AccountName,
            Email = message.Payload.AccountEmail,
            PhoneNumber = message.Payload.PhoneNumber,
        };
        _context.Users.Add(newAccount);
        await _context.SaveChangesAsync();
    }
}
