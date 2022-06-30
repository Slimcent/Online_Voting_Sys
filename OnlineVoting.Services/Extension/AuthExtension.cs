using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace OnlineVoting.Services.Extension
{
    public static class AuthExtension
    {
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null)
                opts = new PasswordOptions()
                {
                    RequiredLength = 6,
                    RequiredUniqueChars = 0,
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };

            string[] randomChars = new[]
            {
                    "ABCDEFGHJKLMNOPQRSTUVWXYZ", // uppercase 
                    "abcdefghijkmnopqrstuvwxyz", // lowercase
                    "0123456789", // digits
                    "!@$?_-" // non-alphanumeric
                };

            CryptoRandom rand = new CryptoRandom();
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count;
                 i < opts.RequiredLength
                 || chars.Distinct().Count() < opts.RequiredUniqueChars;
                 i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            var password = new string(chars.ToArray());
            return password;
        }
    }
}
