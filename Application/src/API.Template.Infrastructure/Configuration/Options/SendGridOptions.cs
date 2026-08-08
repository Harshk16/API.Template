using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Binds from the "SendGrid" config section. ApiKey resolves from Key
    /// Vault / User Secrets only. FromEmail/FromName are non-secret and
    /// may come from appsettings.json or be overridden via the DB
    /// AppSettings table per environment.
    /// </summary>
    public sealed class SendGridOptions
    {
        public const string SectionName = "SendGrid";

        public string ApiKey { get; init; } = string.Empty;

        public string FromEmail { get; init; } = string.Empty;

        public string FromName { get; init; } = string.Empty;
    }
}
