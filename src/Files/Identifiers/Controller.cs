using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Journal.Files.Identifiers;
using Journal.Models.PaginationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Security.Claims;

namespace Journal.Files.Identifiers;

[ApiController]
[Authorize]
[Route("api/files/identifiers")]
public class Controller : ControllerBase
{
    private readonly BlobContainerClient _blobContainerClient;

    public Controller(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    [HttpGet("sas-token")]
    public ActionResult<Uri> CreateServiceSASContainer([FromQuery]string? identifierId)
    {
        // Check if BlobContainerClient object has been authorized with Shared Key
        if (_blobContainerClient.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for one day
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _blobContainerClient.Name,
                Resource = "c"
            };

            if (identifierId == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(2);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List);
            }
            else
            {
                sasBuilder.Identifier = identifierId;
            }

            Uri sasUri = _blobContainerClient.GenerateSasUri(sasBuilder);
            return Ok(sasUri);
        }
        // Client object is not authorized via Shared Key
        return NotFound("Container client is not authorized with Shared Key");
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] Get.Parameters parameters)
    {
        var containerAccessPolicy = await _blobContainerClient.GetAccessPolicyAsync();
        var query = containerAccessPolicy.Value?.SignedIdentifiers?.ToList();
        if (!string.IsNullOrEmpty(parameters.Id))
            query = query?.Where(p => p.Id == parameters.Id).ToList();
        if (!string.IsNullOrEmpty(parameters.Permissions)) //permissions la string cung co thu tu, nhung parameters lai co the nhap ko theo thu tu
            query = query?.Where(p => p.AccessPolicy.Permissions == parameters.Permissions).ToList();
        if (parameters.PolicyStartsOn.HasValue)
            query = query?.Where(p => p.AccessPolicy.PolicyStartsOn == parameters.PolicyStartsOn).ToList();
        if (parameters.PolicyExpiresOn.HasValue)
            query = query?.Where(p => p.AccessPolicy.PolicyExpiresOn == parameters.PolicyExpiresOn).ToList();
        if (parameters.PageSize.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex.Value >= 0)
            query = query?.Skip(parameters.PageSize.Value * parameters.PageIndex.Value).Take(parameters.PageSize.Value).ToList();

        var paginationResults = new Builder<Azure.Storage.Blobs.Models.BlobSignedIdentifier>()
            .WithIndex(parameters.PageIndex)
            .WithSize(parameters.PageSize)
            .WithTotal(query.Count)
            .WithItems(query)
            .Build();

        return Ok(paginationResults);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Post.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var containerAccessPolicy = await _blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var identifier =
            new Azure.Storage.Blobs.Models.BlobSignedIdentifier
            {
                Id = payload.Id,
                AccessPolicy = new Azure.Storage.Blobs.Models.BlobAccessPolicy
                {
                    Permissions = payload.AccessPolicy.Permissions,
                    PolicyExpiresOn = payload.AccessPolicy.PolicyExpiresOn,
                    PolicyStartsOn = payload.AccessPolicy.PolicyStartsOn,
                }
            };
        identifiers.Add(identifier);

        await _blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);

        return CreatedAtAction(nameof(Get), new { payload.Id }, payload.Id);
    }

    [HttpPut]
    public async Task<ActionResult> Update([FromBody] Update.Payload payload)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var containerAccessPolicy = await _blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var identifier = identifiers.FirstOrDefault(p => p.Id == payload.Id);
        if (identifier == null)
            return NotFound($"Identifier '{payload.Id}' not found.");

        identifier.AccessPolicy = new Azure.Storage.Blobs.Models.BlobAccessPolicy
        {
            Permissions = payload.AccessPolicy.Permissions,
            PolicyExpiresOn = payload.AccessPolicy.PolicyExpiresOn,
            PolicyStartsOn = payload.AccessPolicy.PolicyStartsOn,
        };
        await _blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);
        return NoContent();
    }

    [HttpPatch]
    public async Task<IActionResult> Patch([FromQuery] string id,
                                       [FromBody] JsonPatchDocument<Azure.Storage.Blobs.Models.BlobAccessPolicy> patchDoc,
                                       CancellationToken cancellationToken = default!)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        foreach (var op in patchDoc.Operations)
            if (op.OperationType != OperationType.Replace && op.OperationType != OperationType.Test)
                return BadRequest("Only Replace and Test operations are allowed in this patch request.");

        if (patchDoc is null)
            return BadRequest("Patch document cannot be null.");

        var containerAccessPolicy = await _blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        var identifier = identifiers.FirstOrDefault(p => p.Id == id);
        if (identifier == null)
            return NotFound(new ProblemDetails
            {
                Title = "Identifier not found",
                Detail = $"Identifier with ID {id} does not exist.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        var blobAccessPolicy = identifier.AccessPolicy;

        patchDoc.ApplyTo(blobAccessPolicy);
        await _blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromQuery] Delete.Parameters parameters)
    {
        if (User.Identity is null)
            return Unauthorized();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found");

        var containerAccessPolicy = await _blobContainerClient.GetAccessPolicyAsync();
        var identifiers = containerAccessPolicy.Value?.SignedIdentifiers?.ToList() ?? new List<Azure.Storage.Blobs.Models.BlobSignedIdentifier>();
        if (parameters.IsDeleteAll)
        {
            identifiers.Clear();
            await _blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);
            return NoContent();
        }
        if (string.IsNullOrEmpty(parameters.Id))
            return BadRequest("Id is required.");
        var identifier = identifiers.FirstOrDefault(p => p.Id == parameters.Id);
        if (identifier == null)
            return NotFound($"Identifier '{parameters.Id}' not found.");
        identifiers.Remove(identifier);

        await _blobContainerClient.SetAccessPolicyAsync(permissions: identifiers);
        return NoContent();
    }
}