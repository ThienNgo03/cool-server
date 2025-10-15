using BFF.Databases.App;

namespace BFF.Authentication.Register.Messager;

public class Handler
{
    private readonly JournalDbContext _context;
    public Handler(JournalDbContext context)
    {
        _context = context;
    }
    public async Task Handle(Message message)
    {
        var newAccount = new Databases.App.Tables.User.Table
        {
            Id = message.id,
            Name = message.name,
            Email = message.email,
            PhoneNumber = message.phoneNumber,
            ProfilePicture = message.profilePicture,
        };
        _context.Users.Add(newAccount);
        await _context.SaveChangesAsync();
    }
}
