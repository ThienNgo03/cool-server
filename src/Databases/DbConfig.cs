namespace Journal.Databases
{
    public class DbConfig
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TrustedConnection { get; set; }
        public string TrustServerCertificate { get; set; }

    }
}
