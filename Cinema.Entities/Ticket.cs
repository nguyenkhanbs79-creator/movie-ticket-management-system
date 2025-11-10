namespace Cinema.Entities;

public class Ticket : BaseEntity
{
    public int ShowtimeId { get; set; }

    public int SeatId { get; set; }

    public int UserId { get; set; }

    public decimal Price { get; set; }

    public DateTime SoldAt { get; set; }
}
