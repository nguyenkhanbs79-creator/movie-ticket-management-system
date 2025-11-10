namespace Cinema.Entities;

public class Auditorium : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int SeatRows { get; set; }

    public int SeatCols { get; set; }

    public string? Location { get; set; }
}
