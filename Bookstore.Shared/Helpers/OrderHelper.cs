namespace Bookstore.Shared.Helpers
{
    public class OrderHelper
    {
        public static string GenerateRandomAlphanumeric(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GenerateOrderCode(int userId)
        {
            string randomSuffix = GenerateRandomAlphanumeric(5);
            return$"{userId}-{randomSuffix}";
        }
    }
}
