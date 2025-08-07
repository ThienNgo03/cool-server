using Library;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

public static class Config
{
    /// <summary>
    /// API Configuration constants
    /// </summary>
    public static class ApiConfig
    {
        public const string BaseUrl = "https://localhost:7011/api";
        public const int TimeoutSeconds = 30;
        public const bool IgnoreSslErrors = true;

        // Bearer token for authentication
        public const string BearerToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI3NzM3M0I4RjZBQ0ZFRkJBNzQ1MDUyMTg2QzZENEU3IiwidHlwIjoiYXQrand0In0.eyJpc3MiOiJodHRwczovL2lkZW50aXR5LXNlcnZlci1leWdzZXlnNmhhZjJmNmFmLmVhc3Rhc2lhLTAxLmF6dXJld2Vic2l0ZXMubmV0IiwibmJmIjoxNzU0NDY3MDY3LCJpYXQiOjE3NTQ0NjcwNjcsImV4cCI6MTc1NzA1OTA2Nywic2NvcGUiOlsibGlnaHRuaW5nLWxhbmVzLWFwcCIsIm9wZW5pZCIsInByb2ZpbGUiXSwiYW1yIjpbInB3ZCJdLCJjbGllbnRfaWQiOiJwb3N0bWFuIiwic3ViIjoiOWNiMDE0ZDMtNzBhZi00YzkzLTgwNDMtMmEzMTRkOGE5MGY2IiwiYXV0aF90aW1lIjoxNzU0NDY3MDY3LCJpZHAiOiJsb2NhbCIsImp0aSI6IkEyMDg0MjkyRjAzRjQxOUI2OUI3QzMxN0MzODc4MzE0In0.CiBP9JEtW_G_RAepcqhbuhC_nhukmq17HJevA_yv6aYEkelugNE5jIgHaiWtjLqa22duP-uW7KVz-7mMrUV5wD1bBMuLe2OtNl8Ml8LfPX64ImuqKZVIhqrLoFo15sCDMAPl26vg8w2gExUOmvDSKGBVD8pkJ5aBJN0eByslV8IuFPl-ObMivaRqHd4d7VQ_A0beycimVLizraBgknR9bgRsq8w5Ylq__NNCjyPbQVbHKpIJajJvHYcQW9eBJlaMWG8RqWimLdF7eE6D5wDg1GWj4fwQAyBwQp1SWzL5tP4KouZcI-DT1TURwrbzw7rejyiUZ0faMgTmoJl9Uim1nA";
    }


    /// <summary>
    /// Configure services for demos with standard setup
    /// </summary>
    /// <returns>Configured service provider</returns>
    public static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddApiContext<JournalContext>(options =>
        {
            options.BaseUrl = ApiConfig.BaseUrl;
            options.BearerToken = ApiConfig.BearerToken;
            options.Timeout = TimeSpan.FromSeconds(ApiConfig.TimeoutSeconds);
            options.IgnoreSslErrors = ApiConfig.IgnoreSslErrors;
        });


        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Print standard demo header
    /// </summary>
    public static void PrintDemoHeader(string title, string description, params string[] endpoints)
    {
        Console.WriteLine($"🌟 {title}");
        Console.WriteLine(new string('=', title.Length + 4));
        Console.WriteLine(description);

        if (endpoints.Length > 0)
        {
            Console.WriteLine($"Testing endpoints: {string.Join(", ", endpoints)}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Print standard demo footer with summary
    /// </summary>
    public static void PrintDemoFooter(string demoName, Dictionary<string, object>? stats = null)
    {
        Console.WriteLine($"\n🎉 {demoName} completed successfully!");

        if (stats != null && stats.Any())
        {
            Console.WriteLine("📊 Summary:");
            foreach (var stat in stats)
            {
                Console.WriteLine($"   {stat.Key}: {stat.Value}");
            }
        }

        Console.WriteLine("✨ LINQ queries translated to HTTP API calls perfectly!");
    }

    /// <summary>
    /// Handle demo exceptions with consistent formatting
    /// </summary>
    public static void HandleDemoException(Exception ex, string demoName)
    {
        Console.WriteLine($"❌ Error during {demoName}: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"📋 Inner exception: {ex.InnerException.Message}");
        }
    }
}
