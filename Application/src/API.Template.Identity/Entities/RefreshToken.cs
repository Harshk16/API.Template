using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Entities
{
    public sealed class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        // Stored hashed — never the raw token value, same principle as passwords.
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        public DateTime? RevokedUtc { get; set; }

        // Points to the token that replaced this one, forming a rotation chain.
        // Used to detect reuse of an already-rotated (dead) token.
        public Guid? ReplacedByTokenId { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresUtc;
        public bool IsActive => RevokedUtc is null && !IsExpired;
    }
}
