using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Binds from the "Jwt" config section. SigningKey resolves from Key
    /// Vault / User Secrets only. Issuer/Audience/ExpiryMinutes are
    /// non-secret and may come from appsettings.json or be overridden via
    /// the DB AppSettings table per environment.
    /// </summary>
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string SigningKey { get; init; } = string.Empty;

        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public int ExpiryMinutes { get; init; } = 60;
    }
}
