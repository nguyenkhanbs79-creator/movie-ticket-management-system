using System;
using Cinema.DAL;
using Cinema.Entities;

namespace Cinema.BLL;

public class TicketService
{
    private readonly TicketRepository _ticketRepository;
    private readonly ShowtimeRepository _showtimeRepository;

    public TicketService(TicketRepository ticketRepository, ShowtimeRepository showtimeRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _showtimeRepository = showtimeRepository ?? throw new ArgumentNullException(nameof(showtimeRepository));
    }

    public int SellTicket(int showtimeId, int seatId, int userId, decimal price)
    {
        if (showtimeId <= 0)
        {
            throw new ArgumentException("Showtime id must be greater than zero.", nameof(showtimeId));
        }

        if (seatId <= 0)
        {
            throw new ArgumentException("Seat id must be greater than zero.", nameof(seatId));
        }

        if (userId <= 0)
        {
            throw new ArgumentException("User id must be greater than zero.", nameof(userId));
        }

        if (price < 0)
        {
            throw new ArgumentException("Giá vé không hợp lệ.", nameof(price));
        }

        if (!_showtimeRepository.Exists(showtimeId))
        {
            throw new ArgumentException("Suất chiếu không tồn tại", nameof(showtimeId));
        }

        if (_ticketRepository.IsSeatTaken(showtimeId, seatId))
        {
            throw new InvalidOperationException("Ghế đã bán");
        }

        var ticket = new Ticket
        {
            ShowtimeId = showtimeId,
            SeatId = seatId,
            UserId = userId,
            Price = price,
            SoldAt = DateTime.UtcNow
        };

        _ticketRepository.Insert(ticket);
        return ticket.Id;
    }
}
