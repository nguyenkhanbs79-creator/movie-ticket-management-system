using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cinema.BLL;

namespace Cinema.WinForms;

public class ReportForm : Form
{
    private readonly ReportService _reportService;
    private readonly BindingSource _bindingSource = new();
    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _dtpFrom = new();
    private readonly DateTimePicker _dtpTo = new();
    private readonly Button _btnRevenue = new();
    private readonly Button _btnTop = new();
    private readonly Label _lblTotal = new();
    private readonly ErrorProvider _errorProvider = new();

    public ReportForm(ReportService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        InitializeComponent();
    }

    private void BtnRevenue_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            return;
        }

        try
        {
            var data = _reportService.Revenue(_dtpFrom.Value.Date, _dtpTo.Value.Date).ToList();
            _bindingSource.DataSource = new BindingList<RevenueRow>(data);
            _grid.DataSource = _bindingSource;
            var total = data.Sum(r => r.Revenue);
            _lblTotal.Text = $"Tổng doanh thu: {total:C0}";
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnTop_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            return;
        }

        try
        {
            var data = _reportService.TopMovies(_dtpFrom.Value.Date, _dtpTo.Value.Date).ToList();
            _bindingSource.DataSource = new BindingList<TopMovieRow>(data);
            _grid.DataSource = _bindingSource;
            _lblTotal.Text = $"Top {data.Count} phim theo doanh thu";
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
        Text = "Báo cáo";
        Width = 800;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        var lblFrom = new Label { Text = "Từ ngày", AutoSize = true, Location = new System.Drawing.Point(20, 20) };
        _dtpFrom.Location = new System.Drawing.Point(90, 18);
        _dtpFrom.Format = DateTimePickerFormat.Short;

        var lblTo = new Label { Text = "Đến ngày", AutoSize = true, Location = new System.Drawing.Point(250, 20) };
        _dtpTo.Location = new System.Drawing.Point(320, 18);
        _dtpTo.Format = DateTimePickerFormat.Short;

        _btnRevenue.Text = "Xem doanh thu";
        _btnRevenue.Location = new System.Drawing.Point(480, 16);
        _btnRevenue.Click += BtnRevenue_Click;

        _btnTop.Text = "Top phim";
        _btnTop.Location = new System.Drawing.Point(610, 16);
        _btnTop.Click += BtnTop_Click;

        _lblTotal.Location = new System.Drawing.Point(20, 60);
        _lblTotal.AutoSize = true;

        _grid.Dock = DockStyle.Bottom;
        _grid.Height = 420;
        _grid.AutoGenerateColumns = true;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        Controls.Add(lblFrom);
        Controls.Add(_dtpFrom);
        Controls.Add(lblTo);
        Controls.Add(_dtpTo);
        Controls.Add(_btnRevenue);
        Controls.Add(_btnTop);
        Controls.Add(_lblTotal);
        Controls.Add(_grid);

        _errorProvider.ContainerControl = this;
    }
}
