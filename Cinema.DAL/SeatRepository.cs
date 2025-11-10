using System.Data;
using Cinema.Entities;
using Microsoft.Data.SqlClient;

namespace Cinema.DAL;

public class SeatRepository : IRepository<Seat>
{
    private static string ConnectionString => Db.GetConnectionString();

    public IEnumerable<Seat> GetAll()
    {
        try
        {
            var seats = new List<Seat>();
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT SeatId, AuditoriumId, RowNumber, ColumnNumber, CreatedAt, UpdatedAt FROM dbo.Seats ORDER BY AuditoriumId, RowNumber, ColumnNumber;", connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                seats.Add(MapSeat(reader));
            }

            return seats;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve seats.", ex);
        }
    }

    public Seat? GetById(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT SeatId, AuditoriumId, RowNumber, ColumnNumber, CreatedAt, UpdatedAt FROM dbo.Seats WHERE SeatId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapSeat(reader) : null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve seat by id.", ex);
        }
    }

    public void Insert(Seat entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"INSERT INTO dbo.Seats (AuditoriumId, RowNumber, ColumnNumber, CreatedAt, UpdatedAt) VALUES (@AuditoriumId, @RowNumber, @ColumnNumber, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            var now = DateTime.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            command.Parameters.Add(new SqlParameter("@AuditoriumId", SqlDbType.Int) { Value = entity.AuditoriumId });
            command.Parameters.Add(new SqlParameter("@RowNumber", SqlDbType.Int) { Value = entity.RowNumber });
            command.Parameters.Add(new SqlParameter("@ColumnNumber", SqlDbType.Int) { Value = entity.ColNumber });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entity.CreatedAt });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            connection.Open();
            var id = command.ExecuteScalar();
            if (id is int seatId)
            {
                entity.Id = seatId;
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to insert seat.", ex);
        }
    }

    public void Update(Seat entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"UPDATE dbo.Seats SET AuditoriumId = @AuditoriumId, RowNumber = @RowNumber, ColumnNumber = @ColumnNumber, UpdatedAt = @UpdatedAt WHERE SeatId = @Id;", connection);
            entity.UpdatedAt = DateTime.UtcNow;
            command.Parameters.Add(new SqlParameter("@AuditoriumId", SqlDbType.Int) { Value = entity.AuditoriumId });
            command.Parameters.Add(new SqlParameter("@RowNumber", SqlDbType.Int) { Value = entity.RowNumber });
            command.Parameters.Add(new SqlParameter("@ColumnNumber", SqlDbType.Int) { Value = entity.ColNumber });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = entity.Id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to update seat.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.Seats WHERE SeatId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to delete seat.", ex);
        }
    }

    private static Seat MapSeat(SqlDataReader reader)
    {
        return new Seat
        {
            Id = reader.GetInt32(reader.GetOrdinal("SeatId")),
            AuditoriumId = reader.GetInt32(reader.GetOrdinal("AuditoriumId")),
            RowNumber = reader.GetInt32(reader.GetOrdinal("RowNumber")),
            ColNumber = reader.GetInt32(reader.GetOrdinal("ColumnNumber")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
