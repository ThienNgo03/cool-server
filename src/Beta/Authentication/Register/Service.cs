using Azure.Storage.Blobs;
using Grpc.Core;
using Journal.Beta.Authentication.Register;
using Journal.Databases.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Journal.Beta.Authentication.Register;

public class Service:Method.MethodBase
{
    private readonly IdentityContext _context;
    private readonly ILogger<Service> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMessageBus _messageBus;
    private readonly BlobContainerClient _blobContainerClient;
    public Service(ILogger<Service> logger,
        IdentityContext context,
        UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }
    public override async Task<Result> Register(Payload request, ServerCallContext context)
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

        if (request.ProfilePicture.Length > 0)
        {
            var fileExtension = Path.GetExtension(request.ProfilePictureFilename);
            var uniqueFileName = $"avatars/{Guid.NewGuid()}{fileExtension}";
            var blobClient = _blobContainerClient.GetBlobClient(uniqueFileName);

            using var stream = new MemoryStream(request.ProfilePicture.ToByteArray());
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

        return new Result { };
    }

}
