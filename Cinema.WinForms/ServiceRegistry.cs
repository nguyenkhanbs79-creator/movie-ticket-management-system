using Cinema.BLL;
using Cinema.DAL;
using Cinema.Entities;

namespace Cinema.WinForms;

public class ServiceRegistry
{
    public ServiceRegistry()
    {
        MovieRepository = new MovieRepository();
        ShowtimeRepository = new ShowtimeRepository();
        TicketRepository = new TicketRepository();
        AuditoriumRepository = new AuditoriumRepository();
        SeatRepository = new SeatRepository();
        UserRepository = new UserRepository();

        MovieService = new MovieService(MovieRepository);
        ShowtimeService = new ShowtimeService(ShowtimeRepository);
        TicketService = new TicketService(TicketRepository, ShowtimeRepository);
        AuthService = new AuthService(UserRepository);
        ReportService = new ReportService();
    }

    public MovieRepository MovieRepository { get; }

    public ShowtimeRepository ShowtimeRepository { get; }

    public TicketRepository TicketRepository { get; }

    public AuditoriumRepository AuditoriumRepository { get; }

    public SeatRepository SeatRepository { get; }

    public UserRepository UserRepository { get; }

    public MovieService MovieService { get; }

    public ShowtimeService ShowtimeService { get; }

    public TicketService TicketService { get; }

    public AuthService AuthService { get; }

    public ReportService ReportService { get; }
}
