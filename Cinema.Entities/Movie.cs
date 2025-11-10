using System;

namespace Cinema.Entities;

public class Movie : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Duration { get; set; }

    public decimal TicketPrice { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public string? Rating { get; set; }

    public int Duration { get; set; }

    public decimal TicketPrice { get; set; }
}
