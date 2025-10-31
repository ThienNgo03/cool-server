using Azure.Storage.Blobs;
using System.Threading;

namespace Journal.Authentication.Register.Messager;

public class Handler
{
    private readonly JournalDbContext _context;
    private readonly BlobContainerClient _blobContainerClient;
    public Handler(JournalDbContext context, BlobContainerClient blobContainerClient)
    {
        _context = context;
        _blobContainerClient = blobContainerClient;
    }
    public async Task Handle(Message message)
    {
        var newAccount = new Users.Table
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
