using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cinema.BLL;
using Cinema.Entities;

namespace Cinema.WinForms;

public class MovieForm : Form
{
    private readonly MovieService _movieService;
    private readonly BindingSource _bindingSource = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtTitle = new();
    private readonly TextBox _txtGenre = new();
    private readonly TextBox _txtRating = new();
    private readonly DateTimePicker _dtpRelease = new();
    private readonly NumericUpDown _numDuration = new();
    private readonly NumericUpDown _numPrice = new();
    private readonly Button _btnAdd = new();
    private readonly Button _btnEdit = new();
    private readonly Button _btnDelete = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnClear = new();
    private readonly ErrorProvider _errorProvider = new();
    private int? _currentMovieId;

    public MovieForm(MovieService movieService)
    {
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        InitializeComponent();
        Load += (_, _) => LoadMovies();
    }

    private void LoadMovies()
    {
        try
        {
            var movies = _movieService.GetAll().ToList();
            _bindingSource.DataSource = new BindingList<Movie>(movies);
            _grid.DataSource = _bindingSource;
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnGridSelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is Movie movie)
        {
            _currentMovieId = movie.Id;
            _txtTitle.Text = movie.Title;
            _txtGenre.Text = movie.Genre;
            _txtRating.Text = movie.Rating ?? string.Empty;
            _numDuration.Value = Math.Max(_numDuration.Minimum, Math.Min(_numDuration.Maximum, movie.Duration));
            _numPrice.Value = Math.Max(_numPrice.Minimum, Math.Min(_numPrice.Maximum, movie.TicketPrice));
            if (movie.ReleaseDate.HasValue)
            {
                _dtpRelease.Value = movie.ReleaseDate.Value;
                _dtpRelease.Checked = true;
            }
            else
            {
                _dtpRelease.Checked = false;
            }
        }
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        ClearForm();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is Movie movie)
        {
            _currentMovieId = movie.Id;
        }
        else
        {
            MessageBox.Show(this, "Vui lòng chọn phim để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Movie movie)
        {
            MessageBox.Show(this, "Vui lòng chọn phim để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, "Bạn có chắc chắn muốn xóa phim đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            try
            {
                _movieService.Delete(movie.Id);
                LoadMovies();
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

        var movie = BuildMovieFromInputs();

        try
        {
            if (_currentMovieId.HasValue)
            {
                movie.Id = _currentMovieId.Value;
                _movieService.Update(movie);
            }
            else
            {
                _movieService.Create(movie);
                _currentMovieId = movie.Id;
            }

            LoadMovies();
            MessageBox.Show(this, "Lưu phim thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    private Movie BuildMovieFromInputs()
    {
        var movie = new Movie
        {
            Title = _txtTitle.Text.Trim(),
            Genre = _txtGenre.Text.Trim(),
            Duration = (int)_numDuration.Value,
            TicketPrice = _numPrice.Value,
            Rating = string.IsNullOrWhiteSpace(_txtRating.Text) ? null : _txtRating.Text.Trim()
        };

        if (_dtpRelease.Checked)
        {
            movie.ReleaseDate = _dtpRelease.Value.Date;
        }
        else
        {
            movie.ReleaseDate = null;
        }

        return movie;
    }

    private bool ValidateInputs()
    {
        _errorProvider.Clear();
        var valid = true;

        if (string.IsNullOrWhiteSpace(_txtTitle.Text))
        {
            _errorProvider.SetError(_txtTitle, "Tiêu đề không được trống");
            valid = false;
        }

        if (_numDuration.Value <= 0)
        {
            _errorProvider.SetError(_numDuration, "Thời lượng phải lớn hơn 0");
            valid = false;
        }

        if (_numPrice.Value < 0)
        {
            _errorProvider.SetError(_numPrice, "Giá vé không hợp lệ");
            valid = false;
        }

        return valid;
    }

    private void ClearForm()
    {
        _currentMovieId = null;
        _txtTitle.Clear();
        _txtGenre.Clear();
        _txtRating.Clear();
        _numDuration.Value = 1;
        _numPrice.Value = 0;
        _dtpRelease.Checked = false;
        _txtTitle.Focus();
        _errorProvider.Clear();
    }

    private void InitializeComponent()
    {
        Text = "Quản lý phim";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        _grid.Dock = DockStyle.Top;
        _grid.Height = 250;
        _grid.AutoGenerateColumns = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.DataSource = _bindingSource;
        _grid.SelectionChanged += OnGridSelectionChanged;

        var panel = new Panel
        {
            Dock = DockStyle.Fill
        };

        var lblTitle = new Label { Text = "Tiêu đề", AutoSize = true, Location = new System.Drawing.Point(20, 20) };
        _txtTitle.Location = new System.Drawing.Point(150, 18);
        _txtTitle.Width = 250;

        var lblGenre = new Label { Text = "Thể loại", AutoSize = true, Location = new System.Drawing.Point(20, 60) };
        _txtGenre.Location = new System.Drawing.Point(150, 58);
        _txtGenre.Width = 250;

        var lblDuration = new Label { Text = "Thời lượng (phút)", AutoSize = true, Location = new System.Drawing.Point(20, 100) };
        _numDuration.Location = new System.Drawing.Point(150, 98);
        _numDuration.Minimum = 1;
        _numDuration.Maximum = 1000;

        var lblPrice = new Label { Text = "Giá vé", AutoSize = true, Location = new System.Drawing.Point(20, 140) };
        _numPrice.Location = new System.Drawing.Point(150, 138);
        _numPrice.Minimum = 0;
        _numPrice.Maximum = 1000000;
        _numPrice.DecimalPlaces = 2;
        _numPrice.ThousandsSeparator = true;

        var lblRating = new Label { Text = "Đánh giá", AutoSize = true, Location = new System.Drawing.Point(20, 180) };
        _txtRating.Location = new System.Drawing.Point(150, 178);
        _txtRating.Width = 150;

        var lblRelease = new Label { Text = "Ngày phát hành", AutoSize = true, Location = new System.Drawing.Point(20, 220) };
        _dtpRelease.Location = new System.Drawing.Point(150, 218);
        _dtpRelease.Format = DateTimePickerFormat.Short;
        _dtpRelease.ShowCheckBox = true;

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

        panel.Controls.Add(lblTitle);
        panel.Controls.Add(_txtTitle);
        panel.Controls.Add(lblGenre);
        panel.Controls.Add(_txtGenre);
        panel.Controls.Add(lblDuration);
        panel.Controls.Add(_numDuration);
        panel.Controls.Add(lblPrice);
        panel.Controls.Add(_numPrice);
        panel.Controls.Add(lblRating);
        panel.Controls.Add(_txtRating);
        panel.Controls.Add(lblRelease);
        panel.Controls.Add(_dtpRelease);
        panel.Controls.Add(_btnAdd);
        panel.Controls.Add(_btnEdit);
        panel.Controls.Add(_btnDelete);
        panel.Controls.Add(_btnSave);
        panel.Controls.Add(_btnClear);

        Controls.Add(panel);
        Controls.Add(_grid);

        _errorProvider.ContainerControl = this;
    }
}
