namespace Bookstore.Shared.Helpers
{
    public class PasswordHelper
    {
        public static string Hash(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            return passwordHash;
        }

        public static bool Verify(string password, string passwordHash)
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
            return isValid;
        }
    }
}
