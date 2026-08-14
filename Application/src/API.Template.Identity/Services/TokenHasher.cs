using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Services
{
    internal static class TokenHasher
    {
        // SHA256 is fine here — refresh tokens are already high-entropy random
        // values (unlike passwords), so this isn't guarding against brute-force
        // guessing, just avoiding storing the raw secret value in the DB.<
        public static string Hash(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
