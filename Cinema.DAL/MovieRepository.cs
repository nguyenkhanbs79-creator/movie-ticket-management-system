using System.Data;
using Cinema.Entities;
using Microsoft.Data.SqlClient;

namespace Cinema.DAL;

public class MovieRepository : IRepository<Movie>
{
    private static string ConnectionString => Db.GetConnectionString();

    public IEnumerable<Movie> GetAll()
    {
        try
        {
            var movies = new List<Movie>();
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT MovieId, Title, Genre, Duration, TicketPrice, CreatedAt, UpdatedAt FROM dbo.Movies ORDER BY Title;", connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                movies.Add(MapMovie(reader));
            }

            return movies;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve movies.", ex);
        }
    }

    public Movie? GetById(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT MovieId, Title, Genre, Duration, TicketPrice, CreatedAt, UpdatedAt FROM dbo.Movies WHERE MovieId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapMovie(reader) : null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve movie by id.", ex);
        }
    }

    public void Insert(Movie entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"INSERT INTO dbo.Movies (Title, Genre, Duration, TicketPrice, CreatedAt, UpdatedAt) VALUES (@Title, @Genre, @Duration, @TicketPrice, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            var now = DateTime.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = entity.Title });
            command.Parameters.Add(new SqlParameter("@Genre", SqlDbType.NVarChar, 100) { Value = entity.Genre });
            command.Parameters.Add(new SqlParameter("@Duration", SqlDbType.Int) { Value = entity.Duration });
            var priceParam = new SqlParameter("@TicketPrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = entity.TicketPrice };
            command.Parameters.Add(priceParam);
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entity.CreatedAt });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            connection.Open();
            var id = command.ExecuteScalar();
            if (id is int movieId)
            {
                entity.Id = movieId;
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to insert movie.", ex);
        }
    }

    public void Update(Movie entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"UPDATE dbo.Movies SET Title = @Title, Genre = @Genre, Duration = @Duration, TicketPrice = @TicketPrice, UpdatedAt = @UpdatedAt WHERE MovieId = @Id;", connection);
            entity.UpdatedAt = DateTime.UtcNow;
            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = entity.Title });
            command.Parameters.Add(new SqlParameter("@Genre", SqlDbType.NVarChar, 100) { Value = entity.Genre });
            command.Parameters.Add(new SqlParameter("@Duration", SqlDbType.Int) { Value = entity.Duration });
            var priceParam = new SqlParameter("@TicketPrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = entity.TicketPrice };
            command.Parameters.Add(priceParam);
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = entity.Id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to update movie.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.Movies WHERE MovieId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to delete movie.", ex);
        }
    }

    private static Movie MapMovie(SqlDataReader reader)
    {
        return new Movie
        {
            Id = reader.GetInt32(reader.GetOrdinal("MovieId")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Genre = reader.GetString(reader.GetOrdinal("Genre")),
            Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
            TicketPrice = reader.GetDecimal(reader.GetOrdinal("TicketPrice")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
