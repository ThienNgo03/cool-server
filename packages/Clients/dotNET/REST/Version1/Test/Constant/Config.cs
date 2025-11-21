namespace Test.Constant;

public static class Config
{
    public const string JournalConnectionString = "Server=localhost,1433;Database=ssto-database;User Id=sa;Password=SqlServer2022!;TrustServerCertificate=True;";
    #region Cassandra
    //public const string CassandraContactPoint = "localhost";
    //public const int CassandraPort = 9042;
    //public const string CassandraKeyspace = "journal";

    public const string CassandraContactPoint = "";
    public const int CassandraPort = 0;
    public const string CassandraKeyspace = "";
    #endregion
}
