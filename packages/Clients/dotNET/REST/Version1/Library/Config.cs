namespace Library;

public class Config
{
    public string Url { get; set; }

    public string SecretKey { get; set; }

    public bool IsMachineMode { get; set; } = true;

    public Config(string url, 
                  string secretKey, 
                  bool isMachineMode = true)
    {
        Url = url;
        SecretKey = secretKey;
        IsMachineMode = isMachineMode;
    }
}
