﻿using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using local = Test.Constant;
using Test.Databases.Journal;

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

        Library.Config locaHostConfig = new("https://n66lsgdb-7011.asse.devtunnels.ms");
        var services = new ServiceCollection();
        services.AddEndpoints(locaHostConfig);

        services.AddDbContext<JournalDbContext>(options =>
           options.UseSqlServer(local.Config.ConnectionString));

        serviceProvider = services.BuildServiceProvider();

        var tokenService = serviceProvider.GetRequiredService<Library.Token.Service>();
        tokenService.SetToken(token);
    }

    #endregion

    public string? GetBearerToken()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://n66lsgdb-7011.asse.devtunnels.ms/api/authentication/login");

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
