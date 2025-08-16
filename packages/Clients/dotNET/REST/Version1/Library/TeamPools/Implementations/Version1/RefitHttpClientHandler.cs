namespace Library.TeamPools.Implementations.Version1;

public class RefitHttpClientHandler : HttpClientHandler
{
    private readonly Library.Models.Authentication.Model authentication;
    public RefitHttpClientHandler(Library.Models.Authentication.Model authentication)
    {
        this.authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authentication.BearerToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
