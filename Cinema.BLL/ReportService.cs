using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Cinema.DAL;
using Cinema.Entities;
using Microsoft.Data.SqlClient;

namespace Cinema.BLL;

public class ReportService : ReportServiceBase
{
    private readonly string _connectionString;

    public ReportService()
        : this(Db.GetConnectionString())
    {
    }

    public ReportService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public IEnumerable<RevenueRow> Revenue(DateTime from, DateTime to)
    {
        ValidateRange(from, to);
        return Query<RevenueRow>(from, to);
    }

    public IEnumerable<TopMovieRow> TopMovies(DateTime from, DateTime to, int topN = 5)
    {
        if (topN <= 0)
        {
            throw new ArgumentException("TopN must be greater than zero.", nameof(topN));
        }

        ValidateRange(from, to);

        try
        {
            var rows = new List<TopMovieRow>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_TopMoviesByRevenue", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add(new SqlParameter("@From", SqlDbType.DateTime2) { Value = from });
            command.Parameters.Add(new SqlParameter("@To", SqlDbType.DateTime2) { Value = to });
            command.Parameters.Add(new SqlParameter("@TopN", SqlDbType.Int) { Value = topN });
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var movieId = reader.GetInt32(reader.GetOrdinal("MovieId"));
                var title = reader.GetString(reader.GetOrdinal("Title"));
                var revenue = reader.GetDecimal(reader.GetOrdinal("Revenue"));
                var tickets = reader.GetInt32(reader.GetOrdinal("Tickets"));
                rows.Add(new TopMovieRow(movieId, title, revenue, tickets));
            }

            return rows;
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Failed to query top movies.", ex);
        }
    }

    protected override IEnumerable<T> Query<T>(DateTime from, DateTime to)
    {
        if (typeof(T) != typeof(RevenueRow))
        {
            throw new NotSupportedException("Unsupported report type.");
        }

        try
        {
            var rows = new List<RevenueRow>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(@"SELECT SaleDate, TotalRevenue, TotalTickets FROM dbo.vw_RevenueDaily WHERE SaleDate BETWEEN @From AND @To ORDER BY SaleDate;", connection);
            command.Parameters.Add(new SqlParameter("@From", SqlDbType.DateTime2) { Value = from });
            command.Parameters.Add(new SqlParameter("@To", SqlDbType.DateTime2) { Value = to });
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var saleDate = reader.GetDateTime(reader.GetOrdinal("SaleDate"));
                var revenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"));
                var tickets = reader.GetInt32(reader.GetOrdinal("TotalTickets"));
                rows.Add(new RevenueRow(saleDate, revenue, tickets));
            }

            return rows.Cast<T>();
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Failed to query revenue.", ex);
        }
    }
}
