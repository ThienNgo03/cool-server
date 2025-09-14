using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.Journal;


namespace Test.Files;

public class Test: BaseTest
{
    public Test() : base(){ }

    [Fact]
    public async Task Should_Upload_Image_Using_SasToken()
    {
        string sasUri = "http://127.0.0.1:10000/devstoreaccount1/container1?sv=2024-05-04&se=2025-09-13T17%3A51%3A44Z&sr=c&sp=racwdxltfi&sig=p4n%2Bf2VrDvY0VsBmdyTweRtPRaEBofNskpiF1exB124%3D";
        string filePath = @"C:\Users\Thin\source\repos\cool-server\packages\Clients\dotNET\REST\Version1\Test\Files\test.png";
        string fileName = Path.GetFileName(filePath);

        var containerClient = new BlobContainerClient(new Uri(sasUri));
        var blobClient = containerClient.GetBlobClient(fileName);

        using var fileStream = File.OpenRead(filePath);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "image/png"
            },
            Metadata = new Dictionary<string, string>
            {
                { "userId", "12346" }
            },
            //Tags = new Dictionary<string, string>
            //{
            //    { "userId", "12345" }
            //}
        };

        var response = await blobClient.UploadAsync(fileStream, uploadOptions);

        var tags = new Dictionary<string, string>
        {
            { "userId", "12346" }
        };

        await blobClient.SetTagsAsync(tags);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Value.LastModified > DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task Should_Download_Image_By_Metadata()
    {
        string sasUri = "http://127.0.0.1:10000/devstoreaccount1/container1?sv=2024-05-04&se=2025-09-13T17%3A51%3A44Z&sr=c&sp=racwdxltfi&sig=p4n%2Bf2VrDvY0VsBmdyTweRtPRaEBofNskpiF1exB124%3D";
        string userId = "12346";
        string downloadPath = @"C:\Users\Thin\source\repos\cool-server\packages\Clients\dotNET\REST\Version1\Test\Files\downloaded_by_metadata.png";

        var containerClient = new BlobContainerClient(new Uri(sasUri));

        BlobClient targetBlobClient = null;

        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(BlobTraits.Metadata))
        {
            if (blobItem.Metadata.TryGetValue("userId", out var value) && value == userId)
            {
                targetBlobClient = containerClient.GetBlobClient(blobItem.Name);
                break;
            }
        }

        Assert.NotNull(targetBlobClient);

        BlobDownloadInfo download = await targetBlobClient.DownloadAsync();

        using var fileStream = File.OpenWrite(downloadPath);
        await download.Content.CopyToAsync(fileStream);

        Assert.True(File.Exists(downloadPath));
        var fileInfo = new FileInfo(downloadPath);
        Assert.True(fileInfo.Length > 0);
    }

    [Fact]
    public async Task GET()
    {
        var blobContainerClient = serviceProvider!.GetRequiredService<BlobContainerClient>();
        var containerAccessPolicy = await blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var id = "test identifier";
        var permisions = "rwdl";
        var policyStartsOn = DateTimeOffset.UtcNow;
        var policyExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var identifier =
            new Azure.Storage.Blobs.Models.BlobSignedIdentifier
            {
                Id = id,
                AccessPolicy = new Azure.Storage.Blobs.Models.BlobAccessPolicy
                {
                    Permissions = permisions,
                    PolicyExpiresOn = policyExpiresOn,
                    PolicyStartsOn = policyStartsOn,
                }
            };
        identifiers.Add(identifier);

        await blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);

        var identifiersEndpoint = serviceProvider!.GetRequiredService<Library.Files.Identifiers.Interface>();
        var result = await identifiersEndpoint.AllAsync(new()
        {
            PageIndex = 0,
            PageSize = 10
        });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.True(result.Data.Items.Count > 0, "Expected at least one workout in result.");

        identifiers.Remove(identifier);
        await blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);
    }

    [Fact]

    public async Task POST()
    {
        var id = "test identifier";
        var permisions = "rwdl";
        var policyStartsOn = DateTimeOffset.UtcNow;
        var policyExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var payload =
            new Library.Files.Identifiers.Create.Payload
            {
                Id = id,
                AccessPolicy = new Library.Files.Identifiers.Create.BlobAccessPolicy()
                {
                    Permissions = permisions,
                    PolicyExpiresOn = policyExpiresOn,
                    PolicyStartsOn = policyStartsOn,
                }
            };

        var identifiersEndpoint = serviceProvider!.GetRequiredService<Library.Files.Identifiers.Interface>();
        await identifiersEndpoint.CreateAsync(payload);

        var blobContainerClient = serviceProvider!.GetRequiredService<BlobContainerClient>();
        var containerAccessPolicy = await blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var identifier = identifiers.FirstOrDefault(i => i.Id == id);

        Assert.NotNull(identifier);
        Assert.Equal(id, identifier.Id);
        Assert.Equal(permisions, identifier.AccessPolicy.Permissions);
        Assert.Equal(policyStartsOn, identifier.AccessPolicy.PolicyStartsOn);
        Assert.Equal(policyExpiresOn, identifier.AccessPolicy.PolicyExpiresOn);
        identifiers.Remove(identifier);
        await blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);
    }

    [Fact]

    public async Task PUT()
    {
        var blobContainerClient = serviceProvider!.GetRequiredService<BlobContainerClient>();
        var containerAccessPolicy = await blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var id = "test identifier";
        var permisions = "rwdl";
        var policyStartsOn = DateTimeOffset.UtcNow;
        var policyExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var identifier =
            new Azure.Storage.Blobs.Models.BlobSignedIdentifier
            {
                Id = id,
                AccessPolicy = new Azure.Storage.Blobs.Models.BlobAccessPolicy
                {
                    Permissions = permisions,
                    PolicyExpiresOn = policyExpiresOn,
                    PolicyStartsOn = policyStartsOn,
                }
            };
        identifiers.Add(identifier);
        await blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);

        var updatedPermisions = "rwdlc";
        var updatedPolicyStartsOn = DateTimeOffset.UtcNow;
        var updatedPolicyExpiresOn = policyExpiresOn.AddHours(1);
        var payload =
            new Library.Files.Identifiers.Update.Payload
            {
                Id = id,
                AccessPolicy = new Library.Files.Identifiers.Update.BlobAccessPolicy()
                {
                    Permissions = updatedPermisions,
                    PolicyExpiresOn = updatedPolicyExpiresOn,
                    PolicyStartsOn = updatedPolicyStartsOn,
                }
            };
        var identifiersEndpoint = serviceProvider!.GetRequiredService<Library.Files.Identifiers.Interface>();
        await identifiersEndpoint.UpdateAsync(payload);

        var updatedContainerAccessPolicy = await blobContainerClient.GetAccessPolicyAsync();
        var updatedIdentifiers = updatedContainerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var updatedIdentifier = updatedIdentifiers.FirstOrDefault(i => i.Id == id);

        Assert.NotNull(updatedIdentifier);
        Assert.Equal(id, updatedIdentifier.Id);
        //Assert.Equal(updatedPermisions, identifier.AccessPolicy.Permissions);
        Assert.Equal(updatedPolicyStartsOn, updatedIdentifier.AccessPolicy.PolicyStartsOn);
        Assert.Equal(updatedPolicyExpiresOn, updatedIdentifier.AccessPolicy.PolicyExpiresOn);

        updatedIdentifiers.Remove(updatedIdentifier);
        await blobContainerClient.SetAccessPolicyAsync(permissions: updatedIdentifiers);
    }

    [Fact]
    public async Task Delete()
    {
        var blobContainerClient = serviceProvider!.GetRequiredService<BlobContainerClient>();
        var containerAccessPolicy = await blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var id = "test identifier";
        var permisions = "rwdl";
        var policyStartsOn = DateTimeOffset.UtcNow;
        var policyExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var identifier =
            new Azure.Storage.Blobs.Models.BlobSignedIdentifier
            {
                Id = id,
                AccessPolicy = new Azure.Storage.Blobs.Models.BlobAccessPolicy
                {
                    Permissions = permisions,
                    PolicyExpiresOn = policyExpiresOn,
                    PolicyStartsOn = policyStartsOn,
                }
            };
        identifiers.Add(identifier);

        await blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);

        var identifiersEndpoint = serviceProvider!.GetRequiredService<Library.Files.Identifiers.Interface>();
        await identifiersEndpoint.DeleteAsync(new()
        {
            Id = id
        });

        var updatedContainerAccessPolicy = await blobContainerClient.GetAccessPolicyAsync();
        var updatedIdentifiers = updatedContainerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var deletedIdentifier = updatedIdentifiers.FirstOrDefault(i => i.Id == id);

        Assert.Null(deletedIdentifier);
    }

}
