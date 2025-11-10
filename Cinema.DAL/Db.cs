using System.Configuration;

namespace Cinema.DAL;

public static class Db
{
    public static string GetConnectionString()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["CinemaDb"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ApplicationException("Connection string 'CinemaDb' is not configured.");
        }

        return connectionString;
    }
}
