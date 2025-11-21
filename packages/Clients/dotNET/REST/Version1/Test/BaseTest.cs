using Azure.Storage.Blobs;
using Cassandra;
using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Test.Databases.App;
using Test.Databases.Identity;
using local = Test.Constant;

namespace Test;

public class BaseTest
{
    public IServiceProvider serviceProvider;

    #region [ CTors ]

    public BaseTest()
    {
        string? token = GetBearerToken();
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Failed to retrieve authentication token.");

        var libraryConfig = new Library.Config(
                    url: "https://cph84j5r-7011.asse.devtunnels.ms/",
                    secretKey: "secretKey"
                );
        var services = new ServiceCollection();
        services.AddEndpoints(libraryConfig);

        #region Cassandra
        if ((local.Config.CassandraContactPoint != string.Empty || local.Config.CassandraContactPoint != "")
            &&local.Config.CassandraPort!=0
            &&(local.Config.CassandraKeyspace!=string.Empty||local.Config.CassandraKeyspace!=""))
        {
            Cluster cluster = Cluster.Builder()
                .AddContactPoint(local.Config.CassandraContactPoint)
                .WithPort(local.Config.CassandraPort)
                .Build();

            Cassandra.ISession session = cluster.Connect(local.Config.CassandraKeyspace);
            services.AddSingleton<Databases.CassandraCql.Context>();
            services.AddSingleton(session);
        }
        #endregion

        services.AddDbContext<JournalDbContext>(options =>
           options.UseSqlServer(local.Config.JournalConnectionString));
        services.AddDbContext<IdentityContext>(options =>
           options.UseSqlServer(local.Config.JournalConnectionString));

        serviceProvider = services.BuildServiceProvider();

        var tokenService = serviceProvider.GetRequiredService<Library.Token.Service>();
        tokenService.SetToken(token);
    }

    #endregion

    public string? GetBearerToken()
    {
        var client = new HttpClient();
        //var request = new HttpRequestMessage(HttpMethod.Post, "https://storm-ergshka6h7a0bngn.southeastasia-01.azurewebsites.net/api/authentication/login");
        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7011/api/authentication/login");
        var jsonPayload = @"{
            ""account"": ""systemtester@journal.com"",
            ""password"": ""NewPassword@1""
        }";

        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = client.Send(request);
        response.EnsureSuccessStatusCode();

        var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        using var document = JsonDocument.Parse(responseBody);
        var token = document.RootElement.GetProperty("token").GetString();

        return token;
    }
}
