using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cinema.BLL;
using Cinema.DAL;
using Cinema.Entities;

namespace Cinema.WinForms;

public class SearchForm : Form
{
    private readonly MovieService _movieService;
    private readonly ShowtimeService _showtimeService;
    private readonly AuditoriumRepository _auditoriumRepository;
    private readonly BindingSource _bindingSource = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtKeyword = new();
    private readonly ComboBox _cboGenre = new();
    private readonly DateTimePicker _dtpFrom = new();
    private readonly DateTimePicker _dtpTo = new();
    private readonly ComboBox _cboAuditorium = new();
    private readonly Button _btnSearch = new();
    private readonly ErrorProvider _errorProvider = new();
    private List<Movie> _movies = new();
    private List<Auditorium> _auditoriums = new();

    public SearchForm(MovieService movieService, ShowtimeService showtimeService, AuditoriumRepository auditoriumRepository)
    {
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        _showtimeService = showtimeService ?? throw new ArgumentNullException(nameof(showtimeService));
        _auditoriumRepository = auditoriumRepository ?? throw new ArgumentNullException(nameof(auditoriumRepository));
        InitializeComponent();
        Load += (_, _) => InitializeData();
    }

    private void InitializeData()
    {
        try
        {
            _movies = _movieService.GetAll().ToList();
            _auditoriums = _auditoriumRepository.GetAll().ToList();
            var genres = _movies.Select(m => m.Genre).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(g => g).ToList();
            genres.Insert(0, "Tất cả");
            _cboGenre.DataSource = genres;

            var auditoriumItems = new List<Auditorium?> { null };
            auditoriumItems.AddRange(_auditoriums);
            _cboAuditorium.DataSource = auditoriumItems;
            _cboAuditorium.DisplayMember = nameof(Auditorium.Name);
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            return;
        }

        try
        {
            var keyword = _txtKeyword.Text.Trim();
            var genre = _cboGenre.SelectedItem as string;
            var from = _dtpFrom.Value.Date;
            var to = _dtpTo.Value.Date.AddDays(1).AddTicks(-1);
            var auditorium = _cboAuditorium.SelectedItem as Auditorium;

            var movieLookup = _movies.ToDictionary(m => m.Id);
            var auditoriumLookup = _auditoriums.ToDictionary(a => a.Id);

            var showtimes = _showtimeService.GetAll();
            var filtered = new List<SearchResult>();
            foreach (var s in showtimes)
            {
                if (!movieLookup.TryGetValue(s.MovieId, out var movie))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(keyword) && !movie.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(genre) && genre != "Tất cả" && !movie.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (s.StartTime < from || s.StartTime > to)
                {
                    continue;
                }

                if (auditorium != null && s.AuditoriumId != auditorium.Id)
                {
                    continue;
                }

                var roomName = auditoriumLookup.TryGetValue(s.AuditoriumId, out var room) ? room.Name : string.Empty;
                filtered.Add(new SearchResult
                {
                    MovieTitle = movie.Title,
                    Genre = movie.Genre,
                    Auditorium = roomName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    BasePrice = s.BasePrice
                });
            }

            _bindingSource.DataSource = new BindingList<SearchResult>(filtered.OrderBy(r => r.StartTime).ToList());
            _grid.DataSource = _bindingSource;
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateInputs()
    {
        _errorProvider.Clear();
        if (_dtpFrom.Value.Date > _dtpTo.Value.Date)
        {
            _errorProvider.SetError(_dtpTo, "Ngày kết thúc phải sau ngày bắt đầu");
            return false;
        }

        return true;
    }

    private void InitializeComponent()
    {
        Text = "Tìm kiếm";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        var lblKeyword = new Label { Text = "Tên phim", AutoSize = true, Location = new System.Drawing.Point(20, 20) };
        _txtKeyword.Location = new System.Drawing.Point(110, 18);
        _txtKeyword.Width = 200;

        var lblGenre = new Label { Text = "Thể loại", AutoSize = true, Location = new System.Drawing.Point(330, 20) };
        _cboGenre.Location = new System.Drawing.Point(400, 18);
        _cboGenre.Width = 160;
        _cboGenre.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblFrom = new Label { Text = "Từ ngày", AutoSize = true, Location = new System.Drawing.Point(20, 60) };
        _dtpFrom.Location = new System.Drawing.Point(110, 58);
        _dtpFrom.Format = DateTimePickerFormat.Short;

        var lblTo = new Label { Text = "Đến ngày", AutoSize = true, Location = new System.Drawing.Point(330, 60) };
        _dtpTo.Location = new System.Drawing.Point(400, 58);
        _dtpTo.Format = DateTimePickerFormat.Short;

        var lblAuditorium = new Label { Text = "Phòng", AutoSize = true, Location = new System.Drawing.Point(20, 100) };
        _cboAuditorium.Location = new System.Drawing.Point(110, 98);
        _cboAuditorium.Width = 200;
        _cboAuditorium.DropDownStyle = ComboBoxStyle.DropDownList;

        _btnSearch.Text = "Tìm";
        _btnSearch.Location = new System.Drawing.Point(330, 95);
        _btnSearch.Click += BtnSearch_Click;

        _grid.Dock = DockStyle.Bottom;
        _grid.Height = 380;
        _grid.AutoGenerateColumns = true;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        Controls.Add(lblKeyword);
        Controls.Add(_txtKeyword);
        Controls.Add(lblGenre);
        Controls.Add(_cboGenre);
        Controls.Add(lblFrom);
        Controls.Add(_dtpFrom);
        Controls.Add(lblTo);
        Controls.Add(_dtpTo);
        Controls.Add(lblAuditorium);
        Controls.Add(_cboAuditorium);
        Controls.Add(_btnSearch);
        Controls.Add(_grid);

        _errorProvider.ContainerControl = this;
    }

    private class SearchResult
    {
        public string MovieTitle { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public string Auditorium { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal BasePrice { get; set; }
    }
}
