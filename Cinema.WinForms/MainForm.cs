using System;
using System.Windows.Forms;
using Cinema.Entities;

namespace Cinema.WinForms;

public class MainForm : Form
{
    private readonly ServiceRegistry _services;
    private readonly User _currentUser;
    private readonly MenuStrip _menuStrip = new();

    public MainForm(ServiceRegistry services, User currentUser)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        InitializeComponent();
    }

    private void OpenMovies(object? sender, EventArgs e)
    {
        using var form = new MovieForm(_services.MovieService);
        form.ShowDialog(this);
    }

    private void OpenShowtimes(object? sender, EventArgs e)
    {
        using var form = new ShowtimeForm(_services.ShowtimeService, _services.MovieService, _services.AuditoriumRepository);
        form.ShowDialog(this);
    }

    private void OpenTickets(object? sender, EventArgs e)
    {
        using var form = new TicketForm(_services, _currentUser);
        form.ShowDialog(this);
    }

    private void OpenSearch(object? sender, EventArgs e)
    {
        using var form = new SearchForm(_services.MovieService, _services.ShowtimeService, _services.AuditoriumRepository);
        form.ShowDialog(this);
    }

    private void OpenReport(object? sender, EventArgs e)
    {
        using var form = new ReportForm(_services.ReportService);
        form.ShowDialog(this);
    }

    private void Logout(object? sender, EventArgs e)
    {
        Close();
    }

    private void InitializeComponent()
    {
        Text = "Cinema - Quản lý";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;

        var movieMenu = new ToolStripMenuItem("Phim");
        movieMenu.Click += OpenMovies;

        var showtimeMenu = new ToolStripMenuItem("Suất chiếu");
        showtimeMenu.Click += OpenShowtimes;

        var ticketMenu = new ToolStripMenuItem("Bán vé");
        ticketMenu.Click += OpenTickets;

        var searchMenu = new ToolStripMenuItem("Tìm kiếm");
        searchMenu.Click += OpenSearch;

        var reportMenu = new ToolStripMenuItem("Báo cáo");
        reportMenu.Click += OpenReport;

        var logoutMenu = new ToolStripMenuItem("Đăng xuất");
        logoutMenu.Click += Logout;

        _menuStrip.Items.Add(movieMenu);
        _menuStrip.Items.Add(showtimeMenu);
        _menuStrip.Items.Add(ticketMenu);
        _menuStrip.Items.Add(searchMenu);
        _menuStrip.Items.Add(reportMenu);
        _menuStrip.Items.Add(logoutMenu);

        Controls.Add(_menuStrip);
        MainMenuStrip = _menuStrip;
    }
}
