using System.Data;
using Cinema.Entities;
using Microsoft.Data.SqlClient;

namespace Cinema.DAL;

public class TicketRepository : IRepository<Ticket>
{
    private static string ConnectionString => Db.GetConnectionString();

    public IEnumerable<Ticket> GetAll()
    {
        try
        {
            var tickets = new List<Ticket>();
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT TicketId, ShowtimeId, SeatId, UserId, TicketPrice, SoldAt, CreatedAt, UpdatedAt FROM dbo.Tickets ORDER BY SoldAt DESC;", connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tickets.Add(MapTicket(reader));
            }

            return tickets;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve tickets.", ex);
        }
    }

    public Ticket? GetById(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT TicketId, ShowtimeId, SeatId, UserId, TicketPrice, SoldAt, CreatedAt, UpdatedAt FROM dbo.Tickets WHERE TicketId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapTicket(reader) : null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve ticket by id.", ex);
        }
    }

    public void Insert(Ticket entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"INSERT INTO dbo.Tickets (ShowtimeId, SeatId, UserId, TicketPrice, SoldAt, CreatedAt, UpdatedAt) VALUES (@ShowtimeId, @SeatId, @UserId, @TicketPrice, @SoldAt, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            var now = DateTime.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            if (entity.SoldAt == default)
            {
                entity.SoldAt = now;
            }
            command.Parameters.Add(new SqlParameter("@ShowtimeId", SqlDbType.Int) { Value = entity.ShowtimeId });
            command.Parameters.Add(new SqlParameter("@SeatId", SqlDbType.Int) { Value = entity.SeatId });
            command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = entity.UserId });
            var priceParam = new SqlParameter("@TicketPrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = entity.Price };
            command.Parameters.Add(priceParam);
            command.Parameters.Add(new SqlParameter("@SoldAt", SqlDbType.DateTime2) { Value = entity.SoldAt });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entity.CreatedAt });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            connection.Open();
            var id = command.ExecuteScalar();
            if (id is int ticketId)
            {
                entity.Id = ticketId;
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to insert ticket.", ex);
        }
    }

    public void Update(Ticket entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"UPDATE dbo.Tickets SET ShowtimeId = @ShowtimeId, SeatId = @SeatId, UserId = @UserId, TicketPrice = @TicketPrice, SoldAt = @SoldAt, UpdatedAt = @UpdatedAt WHERE TicketId = @Id;", connection);
            entity.UpdatedAt = DateTime.UtcNow;
            if (entity.SoldAt == default)
            {
                entity.SoldAt = entity.UpdatedAt;
            }
            command.Parameters.Add(new SqlParameter("@ShowtimeId", SqlDbType.Int) { Value = entity.ShowtimeId });
            command.Parameters.Add(new SqlParameter("@SeatId", SqlDbType.Int) { Value = entity.SeatId });
            command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = entity.UserId });
            var priceParam = new SqlParameter("@TicketPrice", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = entity.Price };
            command.Parameters.Add(priceParam);
            command.Parameters.Add(new SqlParameter("@SoldAt", SqlDbType.DateTime2) { Value = entity.SoldAt });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = entity.Id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to update ticket.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.Tickets WHERE TicketId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to delete ticket.", ex);
        }
    }

    public bool IsSeatTaken(int showtimeId, int seatId)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Tickets WHERE ShowtimeId = @ShowtimeId AND SeatId = @SeatId) THEN 1 ELSE 0 END;", connection);
            command.Parameters.Add(new SqlParameter("@ShowtimeId", SqlDbType.Int) { Value = showtimeId });
            command.Parameters.Add(new SqlParameter("@SeatId", SqlDbType.Int) { Value = seatId });
            connection.Open();
            var result = command.ExecuteScalar();
            return result is int value && value == 1;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to check seat availability.", ex);
        }
    }

    private static Ticket MapTicket(SqlDataReader reader)
    {
        return new Ticket
        {
            Id = reader.GetInt32(reader.GetOrdinal("TicketId")),
            ShowtimeId = reader.GetInt32(reader.GetOrdinal("ShowtimeId")),
            SeatId = reader.GetInt32(reader.GetOrdinal("SeatId")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            Price = reader.GetDecimal(reader.GetOrdinal("TicketPrice")),
            SoldAt = reader.GetDateTime(reader.GetOrdinal("SoldAt")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
