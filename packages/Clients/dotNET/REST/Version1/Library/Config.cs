namespace Library;

public class Config
{
    public string Url { get; set; }

    public string? SecretKey { get; set; }

    public Config(string url, 
                  string? secretKey)
    {
        Url = url;
        SecretKey = secretKey;
    }
}
