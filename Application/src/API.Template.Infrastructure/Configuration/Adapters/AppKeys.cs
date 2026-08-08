using API.Template.Application.Interfaces;
using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Adapters
{
    /// <summary>
    /// Implements IKeys by wrapping IOptionsMonitor&lt;T&gt; (not IOptions&lt;T&gt;)
    /// for each secret-bearing options class. IOptionsMonitor is used
    /// deliberately: if any of these values were ever sourced from a
    /// reloadable provider, callers would see the new value on their next
    /// property read with no restart. In practice these five are Key
    /// Vault/user-secrets only (never the DB table — see the denylist in
    /// DatabaseSettingsConfigurationProvider), but using the monitor
    /// keeps the adapter consistent with AppSettings and costs nothing.
    ///
    /// Registered as a singleton. Application/Domain code depends only on
    /// IKeys — never on this class or on Microsoft.Extensions.Options.
    /// </summary>
    public sealed class AppKeys : IKeys
    {
        private readonly IOptionsMonitor<DatabaseOptions> _database;
        private readonly IOptionsMonitor<SendGridOptions> _sendGrid;
        private readonly IOptionsMonitor<BlobStorageOptions> _blobStorage;
        private readonly IOptionsMonitor<JwtOptions> _jwt;
        private readonly IOptionsMonitor<ExternalApiOptions> _externalApi;

        public AppKeys(
            IOptionsMonitor<DatabaseOptions> database,
            IOptionsMonitor<SendGridOptions> sendGrid,
            IOptionsMonitor<BlobStorageOptions> blobStorage,
            IOptionsMonitor<JwtOptions> jwt,
            IOptionsMonitor<ExternalApiOptions> externalApi)
        {
            _database = database;
            _sendGrid = sendGrid;
            _blobStorage = blobStorage;
            _jwt = jwt;
            _externalApi = externalApi;
        }

        public string DatabaseConnectionString => _database.CurrentValue.ConnectionString;

        public string SendGridApiKey => _sendGrid.CurrentValue.ApiKey;

        public string BlobStorageConnectionString => _blobStorage.CurrentValue.ConnectionString;

        public string JwtSigningKey => _jwt.CurrentValue.SigningKey;

        public string ExternalApiKey => _externalApi.CurrentValue.ApiKey;
    }
}
