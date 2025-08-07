
namespace Library.Context;

public class ApiContextOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string? BearerToken { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool IgnoreSslErrors { get; set; } = false;
    public Action<HttpClient>? ConfigureHttpClient { get; set; }
    public string? HttpClientName { get; set; }

    public ApiContextOptions UseHttpClient(string httpClientName)
    {
        HttpClientName = httpClientName;
        return this;
    }

    public ApiContextOptions SetBaseUrl(string baseUrl)
    {
        BaseUrl = baseUrl;
        return this;
    }
}
