namespace Cinema.Entities;

public class Showtime : BaseEntity
{
    public int MovieId { get; set; }

    public int AuditoriumId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal BasePrice { get; set; }
}
