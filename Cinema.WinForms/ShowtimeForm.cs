using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cinema.BLL;
using Cinema.DAL;
using Cinema.Entities;

namespace Cinema.WinForms;

public class ShowtimeForm : Form
{
    private readonly ShowtimeService _showtimeService;
    private readonly MovieService _movieService;
    private readonly AuditoriumRepository _auditoriumRepository;
    private readonly BindingSource _bindingSource = new();
    private readonly DataGridView _grid = new();
    private readonly ComboBox _cboMovie = new();
    private readonly ComboBox _cboAuditorium = new();
    private readonly DateTimePicker _dtpStart = new();
    private readonly DateTimePicker _dtpEnd = new();
    private readonly NumericUpDown _numBasePrice = new();
    private readonly Button _btnAdd = new();
    private readonly Button _btnEdit = new();
    private readonly Button _btnDelete = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnClear = new();
    private readonly ErrorProvider _errorProvider = new();
    private int? _currentShowtimeId;

    public ShowtimeForm(ShowtimeService showtimeService, MovieService movieService, AuditoriumRepository auditoriumRepository)
    {
        _showtimeService = showtimeService ?? throw new ArgumentNullException(nameof(showtimeService));
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        _auditoriumRepository = auditoriumRepository ?? throw new ArgumentNullException(nameof(auditoriumRepository));
        InitializeComponent();
        Load += (_, _) => InitializeData();
    }

    private void InitializeData()
    {
        LoadMovies();
        LoadAuditoriums();
        LoadShowtimes();
    }

    private void LoadMovies()
    {
        try
        {
            var movies = _movieService.GetAll().ToList();
            _cboMovie.DataSource = movies;
            _cboMovie.DisplayMember = nameof(Movie.Title);
            _cboMovie.ValueMember = nameof(Movie.Id);
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadAuditoriums()
    {
        try
        {
            var auditoriums = _auditoriumRepository.GetAll().ToList();
            _cboAuditorium.DataSource = auditoriums;
            _cboAuditorium.DisplayMember = nameof(Auditorium.Name);
            _cboAuditorium.ValueMember = nameof(Auditorium.Id);
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadShowtimes()
    {
        try
        {
            var movies = _movieService.GetAll().ToDictionary(m => m.Id);
            var auditoriums = _auditoriumRepository.GetAll().ToDictionary(a => a.Id);
            var showtimes = _showtimeService.GetAll()
                .Select(s => new ShowtimeDisplay
                {
                    Id = s.Id,
                    MovieId = s.MovieId,
                    MovieTitle = movies.TryGetValue(s.MovieId, out var movie) ? movie.Title : string.Empty,
                    AuditoriumId = s.AuditoriumId,
                    AuditoriumName = auditoriums.TryGetValue(s.AuditoriumId, out var room) ? room.Name : string.Empty,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    BasePrice = s.BasePrice
                })
                .OrderBy(s => s.StartTime)
                .ToList();
            _bindingSource.DataSource = new BindingList<ShowtimeDisplay>(showtimes);
            _grid.DataSource = _bindingSource;
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnGridSelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is ShowtimeDisplay display)
        {
            _currentShowtimeId = display.Id;
            _cboMovie.SelectedValue = display.MovieId;
            _cboAuditorium.SelectedValue = display.AuditoriumId;
            _dtpStart.Value = display.StartTime;
            _dtpEnd.Value = display.EndTime;
            _numBasePrice.Value = Math.Max(_numBasePrice.Minimum, Math.Min(_numBasePrice.Maximum, display.BasePrice));
        }
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        ClearForm();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is ShowtimeDisplay display)
        {
            _currentShowtimeId = display.Id;
        }
        else
        {
            MessageBox.Show(this, "Vui lòng chọn suất chiếu để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not ShowtimeDisplay display)
        {
            MessageBox.Show(this, "Vui lòng chọn suất chiếu để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, "Xóa suất chiếu đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            try
            {
                _showtimeService.Delete(display.Id);
                LoadShowtimes();
                ClearForm();
            }
            catch (Exception ex) when (ex is ApplicationException or ArgumentException)
            {
                MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            return;
        }

        var showtime = new Showtime
        {
            MovieId = (int)_cboMovie.SelectedValue,
            AuditoriumId = (int)_cboAuditorium.SelectedValue,
            StartTime = _dtpStart.Value,
            EndTime = _dtpEnd.Value,
            BasePrice = _numBasePrice.Value
        };

        try
        {
            if (_currentShowtimeId.HasValue)
            {
                showtime.Id = _currentShowtimeId.Value;
                _showtimeService.Update(showtime);
            }
            else
            {
                _showtimeService.Create(showtime);
                _currentShowtimeId = showtime.Id;
            }

            LoadShowtimes();
            MessageBox.Show(this, "Lưu suất chiếu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is ApplicationException or ArgumentException)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        ClearForm();
    }

    private bool ValidateInputs()
    {
        _errorProvider.Clear();
        var valid = true;

        if (_dtpStart.Value >= _dtpEnd.Value)
        {
            _errorProvider.SetError(_dtpEnd, "Giờ kết thúc phải sau giờ bắt đầu");
            valid = false;
        }

        if (_numBasePrice.Value < 0)
        {
            _errorProvider.SetError(_numBasePrice, "Giá cơ sở không hợp lệ");
            valid = false;
        }

        if (_cboMovie.SelectedItem is null)
        {
            _errorProvider.SetError(_cboMovie, "Chọn phim");
            valid = false;
        }

        if (_cboAuditorium.SelectedItem is null)
        {
            _errorProvider.SetError(_cboAuditorium, "Chọn phòng chiếu");
            valid = false;
        }

        return valid;
    }

    private void ClearForm()
    {
        _currentShowtimeId = null;
        if (_cboMovie.Items.Count > 0)
        {
            _cboMovie.SelectedIndex = 0;
        }

        if (_cboAuditorium.Items.Count > 0)
        {
            _cboAuditorium.SelectedIndex = 0;
        }

        _dtpStart.Value = DateTime.Now;
        _dtpEnd.Value = DateTime.Now.AddHours(2);
        _numBasePrice.Value = 0;
        _errorProvider.Clear();
    }

    private void InitializeComponent()
    {
        Text = "Quản lý suất chiếu";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        _grid.Dock = DockStyle.Top;
        _grid.Height = 250;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.SelectionChanged += OnGridSelectionChanged;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phim", DataPropertyName = nameof(ShowtimeDisplay.MovieTitle), Width = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = nameof(ShowtimeDisplay.AuditoriumName), Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bắt đầu", DataPropertyName = nameof(ShowtimeDisplay.StartTime), Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kết thúc", DataPropertyName = nameof(ShowtimeDisplay.EndTime), Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá cơ sở", DataPropertyName = nameof(ShowtimeDisplay.BasePrice), Width = 120 });

        _grid.DataSource = _bindingSource;

        var panel = new Panel { Dock = DockStyle.Fill };

        var lblMovie = new Label { Text = "Phim", AutoSize = true, Location = new System.Drawing.Point(20, 20) };
        _cboMovie.Location = new System.Drawing.Point(150, 18);
        _cboMovie.Width = 250;
        _cboMovie.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblAuditorium = new Label { Text = "Phòng", AutoSize = true, Location = new System.Drawing.Point(20, 60) };
        _cboAuditorium.Location = new System.Drawing.Point(150, 58);
        _cboAuditorium.Width = 250;
        _cboAuditorium.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblStart = new Label { Text = "Giờ bắt đầu", AutoSize = true, Location = new System.Drawing.Point(20, 100) };
        _dtpStart.Location = new System.Drawing.Point(150, 98);
        _dtpStart.Format = DateTimePickerFormat.Custom;
        _dtpStart.CustomFormat = "dd/MM/yyyy HH:mm";

        var lblEnd = new Label { Text = "Giờ kết thúc", AutoSize = true, Location = new System.Drawing.Point(20, 140) };
        _dtpEnd.Location = new System.Drawing.Point(150, 138);
        _dtpEnd.Format = DateTimePickerFormat.Custom;
        _dtpEnd.CustomFormat = "dd/MM/yyyy HH:mm";

        var lblPrice = new Label { Text = "Giá cơ sở", AutoSize = true, Location = new System.Drawing.Point(20, 180) };
        _numBasePrice.Location = new System.Drawing.Point(150, 178);
        _numBasePrice.Minimum = 0;
        _numBasePrice.Maximum = 1000000;
        _numBasePrice.DecimalPlaces = 2;
        _numBasePrice.ThousandsSeparator = true;

        _btnAdd.Text = "Thêm";
        _btnAdd.Location = new System.Drawing.Point(450, 20);
        _btnAdd.Click += BtnAdd_Click;

        _btnEdit.Text = "Sửa";
        _btnEdit.Location = new System.Drawing.Point(450, 60);
        _btnEdit.Click += BtnEdit_Click;

        _btnDelete.Text = "Xóa";
        _btnDelete.Location = new System.Drawing.Point(450, 100);
        _btnDelete.Click += BtnDelete_Click;

        _btnSave.Text = "Lưu";
        _btnSave.Location = new System.Drawing.Point(450, 140);
        _btnSave.Click += BtnSave_Click;

        _btnClear.Text = "Clear";
        _btnClear.Location = new System.Drawing.Point(450, 180);
        _btnClear.Click += BtnClear_Click;

        panel.Controls.Add(lblMovie);
        panel.Controls.Add(_cboMovie);
        panel.Controls.Add(lblAuditorium);
        panel.Controls.Add(_cboAuditorium);
        panel.Controls.Add(lblStart);
        panel.Controls.Add(_dtpStart);
        panel.Controls.Add(lblEnd);
        panel.Controls.Add(_dtpEnd);
        panel.Controls.Add(lblPrice);
        panel.Controls.Add(_numBasePrice);
        panel.Controls.Add(_btnAdd);
        panel.Controls.Add(_btnEdit);
        panel.Controls.Add(_btnDelete);
        panel.Controls.Add(_btnSave);
        panel.Controls.Add(_btnClear);

        Controls.Add(panel);
        Controls.Add(_grid);

        _errorProvider.ContainerControl = this;
    }

    private class ShowtimeDisplay
    {
        public int Id { get; set; }

        public int MovieId { get; set; }

        public string MovieTitle { get; set; } = string.Empty;

        public int AuditoriumId { get; set; }

        public string AuditoriumName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal BasePrice { get; set; }
    }
}
