using Azure.Storage.Blobs;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Journal.Beta.Authentication.Register;
using Journal.Databases.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Journal.Beta.Authentication.Register;

public class Service : RegisterMethod.RegisterMethodBase
{
    private readonly IdentityContext _context;
    private readonly ILogger<Service> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMessageBus _messageBus;
    private readonly BlobContainerClient _blobContainerClient;
    public Service(ILogger<Service> logger,
        IdentityContext context,
        UserManager<IdentityUser> userManager,
        IMessageBus messageBus,
        BlobContainerClient blobContainerClient)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _messageBus = messageBus;
        _blobContainerClient = blobContainerClient;
    }
    public override async Task<Empty> Register(Payload request, ServerCallContext context)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ConfirmPassword not matched"));
        }

        var newAccount = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(newAccount, request.Password);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new RpcException(new Status(StatusCode.InvalidArgument, error));
        }

        string? avatar = null;

        if (!string.IsNullOrWhiteSpace(request.ProfilePicturePath) && File.Exists(request.ProfilePicturePath))
        {
            var fileExtension = Path.GetExtension(request.ProfilePicturePath);
            var uniqueFileName = $"avatars/{Guid.NewGuid()}{fileExtension}";
            var blobClient = _blobContainerClient.GetBlobClient(uniqueFileName);

            using var stream = File.OpenRead(request.ProfilePicturePath);
            await blobClient.UploadAsync(stream, overwrite: true);

            avatar = blobClient.Uri.ToString();
        }

        await _messageBus.PublishAsync(new Register.Messager.Message(
            Guid.Parse(newAccount.Id),
            avatar,
            request.FirstName + request.LastName,
            request.Email,
            request.PhoneNumber
        ));

        return new Empty { };
    }

}
