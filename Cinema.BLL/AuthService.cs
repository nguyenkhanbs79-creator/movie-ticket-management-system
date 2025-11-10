using System;
using System.Security.Cryptography;
using System.Text;
using Cinema.DAL;

namespace Cinema.BLL;

public class AuthService
{
    private readonly UserRepository _userRepository;

    public AuthService(UserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public bool Login(string username, string passwordPlain)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(passwordPlain))
        {
            return false;
        }

        var user = _userRepository.GetByUsername(username);
        if (user is null)
        {
            return false;
        }

        var passwordHash = Md5(passwordPlain);
        return user.PasswordHash == passwordHash;
    }

    private static string Md5(string s)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(s);
        var hashBytes = md5.ComputeHash(bytes);
        var builder = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
