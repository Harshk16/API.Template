using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Binds from the "Database" config section. ConnectionString always
    /// resolves from Key Vault / User Secrets (never appsettings.json,
    /// never the DB AppSettings table — enforced by the DB provider's
    /// secret-key denylist). CommandTimeoutSeconds has a json default and
    /// MAY be overridden per-environment by a row in dbo.AppSettings.
    /// </summary>
    public sealed class DatabaseOptions
    {
        public const string SectionName = "Database";

        public string Provider { get; init; } = string.Empty;

        public string ConnectionString { get; init; } = string.Empty;

        public int CommandTimeout { get; init; } = 30;

        public bool EnableDetailedErrors { get; init; }

        public bool EnableSensitiveDataLogging { get; init; }

        public bool EnableRetryOnFailure { get; init; }

        public int MaxRetryCount { get; init; } = 5;

        public int MaxRetryDelaySeconds { get; init; } = 30;
    }
}
