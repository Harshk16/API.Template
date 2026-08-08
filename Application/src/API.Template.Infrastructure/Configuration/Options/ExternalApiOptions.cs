using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Binds from the "ExternalApi" config section. ApiKey resolves from
    /// Key Vault / User Secrets only. BaseUrl is non-secret and may come
    /// from appsettings.json or be overridden via the DB AppSettings
    /// table per environment (e.g. a tighter timeout in Production).
    /// </summary>
    public sealed class ExternalApiOptions
    {
        public const string SectionName = "ExternalApi";

        public string ApiKey { get; init; } = string.Empty;

        public string BaseUrl { get; init; } = string.Empty;

        public int TimeoutSeconds { get; init; } = 30;
    }
}
