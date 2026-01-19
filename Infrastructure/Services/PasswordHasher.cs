using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class PasswordHasher :IPasswordHasher
{
    public string HashPassword(string password)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        return hashedPassword;
    }

    public bool VerifyHashedPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}