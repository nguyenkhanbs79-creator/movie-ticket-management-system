using System.Data;
using Cinema.Entities;
using Microsoft.Data.SqlClient;

namespace Cinema.DAL;

public class ShowtimeRepository : IRepository<Showtime>
{
    private static string ConnectionString => Db.GetConnectionString();

    public IEnumerable<Showtime> GetAll()
    {
        try
        {
            var showtimes = new List<Showtime>();
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT ShowtimeId, MovieId, AuditoriumId, StartTime, EndTime, BasePrice, CreatedAt, UpdatedAt FROM dbo.Showtimes ORDER BY StartTime;", connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                showtimes.Add(MapShowtime(reader));
            }

            return showtimes;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve showtimes.", ex);
        }
    }

    public Showtime? GetById(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT ShowtimeId, MovieId, AuditoriumId, StartTime, EndTime, BasePrice, CreatedAt, UpdatedAt FROM dbo.Showtimes WHERE ShowtimeId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapShowtime(reader) : null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve showtime by id.", ex);
        }
    }

    public void Insert(Showtime entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"INSERT INTO dbo.Showtimes (MovieId, AuditoriumId, StartTime, EndTime, BasePrice, CreatedAt, UpdatedAt) VALUES (@MovieId, @AuditoriumId, @StartTime, @EndTime, @BasePrice, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            var now = DateTime.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            command.Parameters.Add(new SqlParameter("@MovieId", SqlDbType.Int) { Value = entity.MovieId });
            command.Parameters.Add(new SqlParameter("@AuditoriumId", SqlDbType.Int) { Value = entity.AuditoriumId });
            command.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.DateTime2) { Value = entity.StartTime });
            command.Parameters.Add(new SqlParameter("@EndTime", SqlDbType.DateTime2) { Value = entity.EndTime });
            var priceParam = new SqlParameter("@BasePrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = entity.BasePrice };
            command.Parameters.Add(priceParam);
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entity.CreatedAt });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            connection.Open();
            var id = command.ExecuteScalar();
            if (id is int showtimeId)
            {
                entity.Id = showtimeId;
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to insert showtime.", ex);
        }
    }

    public void Update(Showtime entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"UPDATE dbo.Showtimes SET MovieId = @MovieId, AuditoriumId = @AuditoriumId, StartTime = @StartTime, EndTime = @EndTime, BasePrice = @BasePrice, UpdatedAt = @UpdatedAt WHERE ShowtimeId = @Id;", connection);
            entity.UpdatedAt = DateTime.UtcNow;
            command.Parameters.Add(new SqlParameter("@MovieId", SqlDbType.Int) { Value = entity.MovieId });
            command.Parameters.Add(new SqlParameter("@AuditoriumId", SqlDbType.Int) { Value = entity.AuditoriumId });
            command.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.DateTime2) { Value = entity.StartTime });
            command.Parameters.Add(new SqlParameter("@EndTime", SqlDbType.DateTime2) { Value = entity.EndTime });
            var priceParam = new SqlParameter("@BasePrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = entity.BasePrice };
            command.Parameters.Add(priceParam);
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = entity.Id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to update showtime.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.Showtimes WHERE ShowtimeId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to delete showtime.", ex);
        }
    }

    public bool Exists(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Showtimes WHERE ShowtimeId = @Id) THEN 1 ELSE 0 END;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            var result = command.ExecuteScalar();
            return result is int value && value == 1;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to check showtime existence.", ex);
        }
    }

    private static Showtime MapShowtime(SqlDataReader reader)
    {
        return new Showtime
        {
            Id = reader.GetInt32(reader.GetOrdinal("ShowtimeId")),
            MovieId = reader.GetInt32(reader.GetOrdinal("MovieId")),
            AuditoriumId = reader.GetInt32(reader.GetOrdinal("AuditoriumId")),
            StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
            EndTime = reader.GetDateTime(reader.GetOrdinal("EndTime")),
            BasePrice = reader.GetDecimal(reader.GetOrdinal("BasePrice")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
