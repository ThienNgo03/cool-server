namespace Test.Constant;

public static class Config
{
    public const string JournalConnectionString = "Server=localhost;Database=Journal;Trusted_Connection=True;TrustServerCertificate=True;";
    public const string IdentityConnectionString = "Server=localhost;Database=Identity;Trusted_Connection=True;TrustServerCertificate=True;";
    #region Cassandra
    //public const string CassandraContactPoint = "localhost";
    //public const int CassandraPort = 9042;
    //public const string CassandraKeyspace = "journal";

    public const string CassandraContactPoint = "";
    public const int CassandraPort = 0;
    public const string CassandraKeyspace = "";
    #endregion
}
