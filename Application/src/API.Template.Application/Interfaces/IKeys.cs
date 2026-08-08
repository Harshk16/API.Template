using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Application.Interfaces
{
    /// <summary>
    /// Secrets only. Every property here resolves from Key Vault
    /// (Staging/Production) or User Secrets (Development) — never from
    /// appsettings.json and never from the DB settings table.
    /// Implemented in Infrastructure; Application only depends on this
    /// contract, never on Microsoft.Extensions.Options.
    /// </summary>
    public interface IKeys
    {
        string DatabaseConnectionString { get; }
        string SendGridApiKey { get; }
        string BlobStorageConnectionString { get; }
        string JwtSigningKey { get; }
        string ExternalApiKey { get; }
    }
}
