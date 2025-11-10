using System.Data;
using Cinema.Entities;
using Microsoft.Data.SqlClient;

namespace Cinema.DAL;

public class UserRepository : IRepository<User>
{
    private static string ConnectionString => Db.GetConnectionString();

    public IEnumerable<User> GetAll()
    {
        try
        {
            var users = new List<User>();
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT UserId, Username, PasswordHash, RoleId, CreatedAt, UpdatedAt FROM dbo.Users ORDER BY Username;", connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve users.", ex);
        }
    }

    public User? GetById(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"SELECT UserId, Username, PasswordHash, RoleId, CreatedAt, UpdatedAt FROM dbo.Users WHERE UserId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapUser(reader) : null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve user by id.", ex);
        }
    }

    public void Insert(User entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"INSERT INTO dbo.Users (Username, PasswordHash, RoleId, CreatedAt, UpdatedAt) VALUES (@Username, @PasswordHash, @RoleId, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            var now = DateTime.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = entity.Username });
            command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 64) { Value = entity.PasswordHash });
            command.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.Int) { Value = entity.RoleId });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entity.CreatedAt });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            connection.Open();
            var id = command.ExecuteScalar();
            if (id is int userId)
            {
                entity.Id = userId;
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to insert user.", ex);
        }
    }

    public void Update(User entity)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(@"UPDATE dbo.Users SET Username = @Username, PasswordHash = @PasswordHash, RoleId = @RoleId, UpdatedAt = @UpdatedAt WHERE UserId = @Id;", connection);
            entity.UpdatedAt = DateTime.UtcNow;
            command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = entity.Username });
            command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 64) { Value = entity.PasswordHash });
            command.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.Int) { Value = entity.RoleId });
            command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = entity.UpdatedAt });
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = entity.Id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to update user.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand("DELETE FROM dbo.Users WHERE UserId = @Id;", connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to delete user.", ex);
        }
    }

    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("UserId")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
