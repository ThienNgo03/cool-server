using System.Net.Http.Headers;
using System.Text;

namespace Journal.Exercises.Get.SuperSearch;

public class RefitHttpClientHandler : HttpClientHandler
{
    private readonly string _username;
    private readonly string _password;

    public RefitHttpClientHandler(string username, string password, bool skipCertificateValidation = false)
    {
        _username = username;
        _password = password;

        if (skipCertificateValidation)
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
        {
            var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
            var base64 = Convert.ToBase64String(byteArray);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}