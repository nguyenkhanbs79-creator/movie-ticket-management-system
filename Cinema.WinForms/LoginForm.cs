using System;
using System.Windows.Forms;
using Cinema.BLL;
using Cinema.Entities;

namespace Cinema.WinForms;

public class LoginForm : Form
{
    private readonly ServiceRegistry _services;
    private readonly AuthService _authService;
    private readonly TextBox _txtUser = new();
    private readonly TextBox _txtPass = new();
    private readonly Button _btnLogin = new();
    private readonly Label _lblTitle = new();

    public LoginForm(ServiceRegistry services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _authService = services.AuthService;
        InitializeComponent();
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var username = _txtUser.Text.Trim();
        var password = _txtPass.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show(this, "Vui lòng nhập tên đăng nhập và mật khẩu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_authService.Login(username, password))
            {
                var user = _services.UserRepository.GetByUsername(username);
                if (user is null)
                {
                    MessageBox.Show(this, "Không tìm thấy thông tin người dùng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ShowMain(user);
            }
            else
            {
                MessageBox.Show(this, "Tên đăng nhập hoặc mật khẩu không đúng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (ApplicationException ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowMain(User user)
    {
        Hide();
        using var main = new MainForm(_services, user);
        main.ShowDialog(this);
        _txtPass.Clear();
        _txtPass.Focus();
        Show();
    }

    private void InitializeComponent()
    {
        Text = "Cinema - Đăng nhập";
        Width = 360;
        Height = 220;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        _lblTitle.Text = "Đăng nhập hệ thống";
        _lblTitle.AutoSize = true;
        _lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        _lblTitle.Location = new System.Drawing.Point(90, 20);

        var lblUser = new Label
        {
            Text = "Tài khoản:",
            AutoSize = true,
            Location = new System.Drawing.Point(40, 70)
        };

        _txtUser.Location = new System.Drawing.Point(140, 68);
        _txtUser.Width = 160;

        var lblPass = new Label
        {
            Text = "Mật khẩu:",
            AutoSize = true,
            Location = new System.Drawing.Point(40, 105)
        };

        _txtPass.Location = new System.Drawing.Point(140, 103);
        _txtPass.Width = 160;
        _txtPass.UseSystemPasswordChar = true;

        _btnLogin.Text = "Đăng nhập";
        _btnLogin.Location = new System.Drawing.Point(140, 140);
        _btnLogin.Width = 160;
        _btnLogin.Click += BtnLogin_Click;

        Controls.Add(_lblTitle);
        Controls.Add(lblUser);
        Controls.Add(_txtUser);
        Controls.Add(lblPass);
        Controls.Add(_txtPass);
        Controls.Add(_btnLogin);

        AcceptButton = _btnLogin;
    }
}
