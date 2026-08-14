using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Extensions
{
    public static partial class IdentityExtensions
    {
        public static string? GetUserClaim(IEnumerable<Claim> claims, string claimName)
        {
            return claims.SingleOrDefault(i => i.Type.Equals(claimName))?.Value;
        }

        public static Guid? GetId(this IEnumerable<Claim> claims)
        {
            var id = GetUserClaim(claims, Constants.CLAIM_ID);
            return string.IsNullOrEmpty(id) ? (Guid?)null : Guid.Parse(id);
        }
    }
}
