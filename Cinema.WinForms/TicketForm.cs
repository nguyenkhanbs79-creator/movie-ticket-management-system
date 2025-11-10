using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cinema.BLL;
using Cinema.Entities;

namespace Cinema.WinForms;

public class TicketForm : Form
{
    private readonly ServiceRegistry _services;
    private readonly TicketService _ticketService;
    private readonly ShowtimeService _showtimeService;
    private readonly MovieService _movieService;
    private readonly User _currentUser;
    private readonly ComboBox _cboShowtime = new();
    private readonly Panel _seatPanel = new();
    private readonly Label _lblInfo = new();
    private readonly ErrorProvider _errorProvider = new();
    private Showtime? _selectedShowtime;
    private Movie? _selectedMovie;

    public TicketForm(ServiceRegistry services, User currentUser)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _ticketService = services.TicketService;
        _showtimeService = services.ShowtimeService;
        _movieService = services.MovieService;
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        InitializeComponent();
        Load += (_, _) => LoadShowtimes();
    }

    private void LoadShowtimes()
    {
        try
        {
            var movies = _movieService.GetAll().ToDictionary(m => m.Id);
            var auditoriums = _services.AuditoriumRepository.GetAll().ToDictionary(a => a.Id);
            var showtimes = _showtimeService.GetAll()
                .OrderBy(s => s.StartTime)
                .Select(s => new ShowtimeOption
                {
                    Showtime = s,
                    Display = movies.TryGetValue(s.MovieId, out var movie) && auditoriums.TryGetValue(s.AuditoriumId, out var room)
                        ? $"{movie.Title} - {room.Name} ({s.StartTime:dd/MM HH:mm})"
                        : $"Suất {s.Id}"
                })
                .ToList();
            _cboShowtime.DataSource = showtimes;
            _cboShowtime.DisplayMember = nameof(ShowtimeOption.Display);
            if (showtimes.Count > 0)
            {
                _cboShowtime.SelectedIndex = 0;
            }
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CboShowtime_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cboShowtime.SelectedItem is ShowtimeOption option)
        {
            _selectedShowtime = option.Showtime;
            _selectedMovie = _movieService.GetById(option.Showtime.MovieId);
            RenderSeats();
        }
    }

    private void RenderSeats()
    {
        _seatPanel.Controls.Clear();
        if (_selectedShowtime is null)
        {
            return;
        }

        try
        {
            var showtime = _showtimeService.GetById(_selectedShowtime.Id);
            if (showtime is null)
            {
                return;
            }

            var auditorium = _services.AuditoriumRepository.GetById(showtime.AuditoriumId);
            if (auditorium is null)
            {
                return;
            }

            var seats = _services.SeatRepository.GetAll()
                .Where(s => s.AuditoriumId == auditorium.Id)
                .ToDictionary(s => (s.RowNumber, s.ColNumber));
            var soldSeats = new HashSet<int>(_services.TicketRepository.GetAll()
                .Where(t => t.ShowtimeId == showtime.Id)
                .Select(t => t.SeatId));

            var table = new TableLayoutPanel
            {
                RowCount = auditorium.SeatRows,
                ColumnCount = auditorium.SeatCols,
                AutoSize = true,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };

            for (int c = 0; c < auditorium.SeatCols; c++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }

            for (int r = 0; r < auditorium.SeatRows; r++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            for (int row = 1; row <= auditorium.SeatRows; row++)
            {
                for (int col = 1; col <= auditorium.SeatCols; col++)
                {
                    if (!seats.TryGetValue((row, col), out var seat))
                    {
                        continue;
                    }

                    var button = new Button
                    {
                        Text = $"{row}-{col}",
                        Margin = new Padding(3),
                        AutoSize = true,
                        Tag = seat
                    };

                    if (soldSeats.Contains(seat.Id))
                    {
                        button.Enabled = false;
                        button.BackColor = Color.LightGray;
                    }
                    else
                    {
                        button.BackColor = Color.LightGreen;
                        button.Click += SeatButton_Click;
                    }

                    table.Controls.Add(button, col - 1, row - 1);
                }
            }

            _seatPanel.Controls.Add(table);
            _lblInfo.Text = BuildInfoText(showtime, auditorium);
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string BuildInfoText(Showtime showtime, Auditorium auditorium)
    {
        var movieTitle = _selectedMovie?.Title ?? "";
        var price = Math.Max(showtime.BasePrice, _selectedMovie?.TicketPrice ?? 0);
        return $"Phim: {movieTitle} - Phòng: {auditorium.Name} - Giá bán: {price:C0}";
    }

    private void SeatButton_Click(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.Tag is not Seat seat || _selectedShowtime is null)
        {
            return;
        }

        var price = Math.Max(_selectedShowtime.BasePrice, _selectedMovie?.TicketPrice ?? 0);
        var message = $"Bán ghế {button.Text} với giá {price:C0}?";
        if (MessageBox.Show(this, message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            try
            {
                _ticketService.SellTicket(_selectedShowtime.Id, seat.Id, _currentUser.Id, price);
                button.Enabled = false;
                button.BackColor = Color.LightGray;
            }
            catch (Exception ex) when (ex is ApplicationException or ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void InitializeComponent()
    {
        Text = "Bán vé";
        Width = 800;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        var lblShowtime = new Label { Text = "Suất chiếu", AutoSize = true, Location = new Point(20, 20) };
        _cboShowtime.Location = new Point(110, 18);
        _cboShowtime.Width = 400;
        _cboShowtime.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboShowtime.SelectedIndexChanged += CboShowtime_SelectedIndexChanged;

        _lblInfo.Location = new Point(20, 60);
        _lblInfo.AutoSize = true;

        _seatPanel.Location = new Point(20, 100);
        _seatPanel.Size = new Size(740, 440);
        _seatPanel.BorderStyle = BorderStyle.FixedSingle;
        _seatPanel.AutoScroll = true;

        Controls.Add(lblShowtime);
        Controls.Add(_cboShowtime);
        Controls.Add(_lblInfo);
        Controls.Add(_seatPanel);

        _errorProvider.ContainerControl = this;
    }

    private class ShowtimeOption
    {
        public string Display { get; set; } = string.Empty;

        public Showtime Showtime { get; set; } = new();
    }
}
