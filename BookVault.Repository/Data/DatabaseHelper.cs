namespace BookVault.Repository.Data
{
    public static class DatabaseHelper
    {
        public static string ConnectionString { get; private set; } = null!;

        public static void Initialize(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}