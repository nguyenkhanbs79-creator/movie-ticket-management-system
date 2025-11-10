using System.Data;
using Cinema.Entities;
using Microsoft.Data.SqlClient;

namespace Cinema.DAL;

public class AuditoriumRepository : IRepository<Auditorium>
{
    private static string ConnectionString => Db.GetConnectionString();

    public IEnumerable<Auditorium> GetAll()
    {
        try
        {
            var auditoriums = new List<Auditorium>();
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT AuditoriumId, Name, SeatRows, SeatCols, Location, CreatedAt, UpdatedAt FROM dbo.Auditoriums ORDER BY Name;", connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                auditoriums.Add(MapAuditorium(reader));
            }

            return auditoriums;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve auditoriums.", ex);
        }
    }

    public Auditorium? GetById(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT AuditoriumId, Name, SeatRows, SeatCols, Location, CreatedAt, UpdatedAt FROM dbo.Auditoriums WHERE AuditoriumId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapAuditorium(reader) : null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve auditorium by id.", ex);
        }
    }

    public void Insert(Auditorium entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"INSERT INTO dbo.Auditoriums (Name, SeatRows, SeatCols, Location, CreatedAt, UpdatedAt) VALUES (@Name, @SeatRows, @SeatCols, @Location, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            var now = DateTime.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = entity.Name });
            command.Parameters.Add(new SqlParameter("@SeatRows", SqlDbType.Int) { Value = entity.SeatRows });
            command.Parameters.Add(new SqlParameter("@SeatCols", SqlDbType.Int) { Value = entity.SeatCols });
            command.Parameters.Add(new SqlParameter("@Location", SqlDbType.NVarChar, 200) { Value = (object?)entity.Location ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entity.CreatedAt });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            connection.Open();
            var id = command.ExecuteScalar();
            if (id is int auditoriumId)
            {
                entity.Id = auditoriumId;
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to insert auditorium.", ex);
        }
    }

    public void Update(Auditorium entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"UPDATE dbo.Auditoriums SET Name = @Name, SeatRows = @SeatRows, SeatCols = @SeatCols, Location = @Location, UpdatedAt = @UpdatedAt WHERE AuditoriumId = @Id;", connection);
            entity.UpdatedAt = DateTime.UtcNow;
            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = entity.Name });
            command.Parameters.Add(new SqlParameter("@SeatRows", SqlDbType.Int) { Value = entity.SeatRows });
            command.Parameters.Add(new SqlParameter("@SeatCols", SqlDbType.Int) { Value = entity.SeatCols });
            command.Parameters.Add(new SqlParameter("@Location", SqlDbType.NVarChar, 200) { Value = (object?)entity.Location ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = entity.Id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to update auditorium.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.Auditoriums WHERE AuditoriumId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to delete auditorium.", ex);
        }
    }

    private static Auditorium MapAuditorium(SqlDataReader reader)
    {
        return new Auditorium
        {
            Id = reader.GetInt32(reader.GetOrdinal("AuditoriumId")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            SeatRows = reader.GetInt32(reader.GetOrdinal("SeatRows")),
            SeatCols = reader.GetInt32(reader.GetOrdinal("SeatCols")),
            Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
