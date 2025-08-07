using Library.Sets;

namespace Library.Context;

public class EndpointBuilder<T> where T : class
{
    #region [ Fields ]

    private readonly HttpClient httpClient;

    private readonly ApiContextOptions options;

    private string? endpoint;

    private string? apiVersion;

    private Dictionary<string, string> customHeaders = new();

    private TimeSpan? customTimeout;

    private Func<HttpClient, IApiSet<T>>? customFactory;

    internal EndpointBuilder(HttpClient httpClient, ApiContextOptions options)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public EndpointBuilder<T> WithEndpoint(string endpoint)
    {
        this.endpoint = endpoint;
        return this;
    }

    public EndpointBuilder<T> WithVersion(string version)
    {
        this.apiVersion = version;
        return this;
    }

    public EndpointBuilder<T> WithHeader(string name, string value)
    {
        this.customHeaders[name] = value;
        return this;
    }

    public EndpointBuilder<T> WithTimeout(TimeSpan timeout)
    {
        this.customTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Use convention-based endpoint naming with smart version handling
    /// </summary>
    public EndpointBuilder<T> WithConventionNaming()
    {
        var entityName = typeof(T).Name.ToLower();

        // ✅ Check if version was set
        if (!string.IsNullOrEmpty(this.apiVersion))
        {
            this.endpoint = $"/api/{this.apiVersion}/{entityName}s";  // With version
        }
        else
        {
            this.endpoint = $"/api/{entityName}s";  // Without version
        }

        return this;
    }

    /// <summary>
    /// Use custom factory for ApiSet creation
    /// </summary>
    public EndpointBuilder<T> WithCustomFactory(Func<HttpClient, IApiSet<T>> factory)
    {
        this.customFactory = factory;
        return this;
    }

    /// <summary>
    /// Build the configured ApiSet with smart version handling
    /// </summary>
    public IApiSet<T> Build()
    {
        if (string.IsNullOrEmpty(this.endpoint))
        {
            throw new InvalidOperationException($"Endpoint not specified for {typeof(T).Name}");
        }

        // Use custom factory if provided
        if (this.customFactory != null)
        {
            return this.customFactory(this.httpClient);
        }

        // ✅ Smart version handling logic
        string fullEndpoint = BuildVersionedEndpoint();

        // Create ApiSet
        var apiSet = new ApiSet<T>(this.httpClient, fullEndpoint);

        // Apply custom configurations if needed
        ApplyCustomConfigurations(apiSet);

        return apiSet;
    }

    private string BuildVersionedEndpoint()
    {
        // Case 1: No version specified - simple concatenation
        if (string.IsNullOrEmpty(this.apiVersion))
        {
            return this.endpoint!.StartsWith("/")
                ? $"{this.options.BaseUrl}{this.endpoint}"     // No version
                : $"{this.options.BaseUrl}/{this.endpoint}";   // No version
        }

        // Case 2: Version specified - smart version handling

        // Check if endpoint already has /api/ prefix
        if (this.endpoint!.StartsWith("/api/"))
        {
            // Already has /api/, use as-is (user manages their own versioning)
            return $"{this.options.BaseUrl}{this.endpoint}";
        }

        // Check if BaseUrl already ends with /api
        var baseUrl = this.options.BaseUrl.TrimEnd('/');
        var hasApiInBase = baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase);

        if (hasApiInBase)
        {
            // BaseUrl has /api, add version directly
            return this.endpoint.StartsWith("/")
                ? $"{baseUrl}/{this.apiVersion}{this.endpoint}"      // BaseUrl/v1/endpoint
                : $"{baseUrl}/{this.apiVersion}/{this.endpoint}";    // BaseUrl/v1/endpoint
        }
        else
        {
            // BaseUrl doesn't have /api, add full path
            return this.endpoint.StartsWith("/")
                ? $"{baseUrl}/api/{this.apiVersion}{this.endpoint}"  // BaseUrl/api/v1/endpoint  
                : $"{baseUrl}/api/{this.apiVersion}/{this.endpoint}"; // BaseUrl/api/v1/endpoint
        }
    }

    private void ApplyCustomConfigurations(IApiSet<T> apiSet)
    {
        // Apply custom headers, timeout, etc.
        // This would require extending ApiSet to support these features

        // For now, this is a placeholder for future enhancements
        foreach (var header in this.customHeaders)
        {
            // TODO: Apply custom headers to ApiSet
            // apiSet.AddHeader(header.Key, header.Value);
        }

        if (this.customTimeout.HasValue)
        {
            // TODO: Apply custom timeout to ApiSet
            // apiSet.SetTimeout(_customTimeout.Value);
        }
    }
    #endregion
}
