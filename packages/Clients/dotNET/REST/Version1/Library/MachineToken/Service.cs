
namespace Library.MachineToken;

public class Service
{
    private string secretKey;

    public Service(Config config)
    {
        this.secretKey = config.SecretKey;
    }

    public string ComputeHash()
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(this.secretKey);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
