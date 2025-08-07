using Library.Sets;

namespace Library.Context;

public abstract class ApiContext : IDisposable
{
    private bool _disposed = false;
    protected readonly HttpClient _httpClient;
    protected readonly ApiContextOptions _options;

    protected ApiContext(ApiContextOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = CreateHttpClient(options);

        // Hook cho derived class register endpoints
        OnEndpointRegistering();
    }

    /// <summary>
    /// Constructor for dependency injection with external HttpClient
    /// </summary>
    protected ApiContext(ApiContextOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Hook cho derived class register endpoints
        OnEndpointRegistering();
    }

    /// <summary>
    /// Hook method for derived classes to register their API endpoints
    /// </summary>
    protected virtual void OnEndpointRegistering() { }

    /// <summary>
    /// Creates an endpoint builder for fluent API configuration
    /// </summary>
    protected EndpointBuilder<T> RegisterEndpoint<T>() where T : class
    {
        return new EndpointBuilder<T>(_httpClient, _options);
    }

    /// <summary>
    /// Simple factory method for basic endpoint creation
    /// </summary>
    protected IApiSet<T> CreateApiSet<T>(string endpoint) where T : class
    {
        var fullEndpoint = endpoint.StartsWith("/")
            ? $"{_options.BaseUrl}{endpoint}"
            : $"{_options.BaseUrl}/{endpoint}";
        return new ApiSet<T>(_httpClient, fullEndpoint);
    }

    // Infrastructure methods giữ nguyên
    private static HttpClient CreateHttpClient(ApiContextOptions options)
    {
        HttpClientHandler? handler = null;

        if (options.IgnoreSslErrors)
        {
            handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        }

        var httpClient = handler != null ? new HttpClient(handler) : new HttpClient();
        httpClient.Timeout = options.Timeout;

        // Set base URL
        if (!string.IsNullOrEmpty(options.BaseUrl))
        {
            httpClient.BaseAddress = new Uri(options.BaseUrl);
        }

        // Set Bearer token
        if (!string.IsNullOrEmpty(options.BearerToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.BearerToken);
        }

        // Custom configuration
        options.ConfigureHttpClient?.Invoke(httpClient);

        return httpClient;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}