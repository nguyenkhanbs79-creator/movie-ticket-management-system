namespace Cinema.Entities;

public class Seat : BaseEntity
{
    public int AuditoriumId { get; set; }

    public int RowNumber { get; set; }

    public int ColNumber { get; set; }
}
