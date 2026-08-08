using API.Template.Application.Interfaces;
using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Adapters
{
    /// <summary>
    /// Implements ISettings by wrapping IOptionsMonitor&lt;T&gt; for the
    /// typed, known settings, plus raw IConfiguration for the
    /// FeatureFlag() escape hatch. IOptionsMonitor matters here more than
    /// in AppKeys: CommandTimeoutSeconds, FromEmail, etc. CAN legitimately
    /// be overridden by a row in dbo.AppSettings, and that provider
    /// reloads on a background timer — callers see the new value on their
    /// next property read, no restart needed.
    ///
    /// FeatureFlag(key) reads IConfiguration directly rather than binding
    /// a typed options class, because flags are added to the DB table
    /// ad-hoc without a code change — there's no fixed set of properties
    /// to bind in advance. IConfiguration.GetValue() is cheap and the
    /// underlying value is kept fresh by the same reload timer.
    ///
    /// Registered as a singleton. Application/Domain code depends only on
    /// ISettings — never on this class or on Microsoft.Extensions.Options.
    /// </summary>
    internal sealed class AppSettings : ISettings
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IOptionsMonitor<DatabaseOptions> _database;
        private readonly IOptionsMonitor<BlobStorageOptions> _blobStorage;
        private readonly IOptionsMonitor<SendGridOptions> _sendGrid;
        private readonly IOptionsMonitor<JwtOptions> _jwt;
        private readonly IOptionsMonitor<ExternalApiOptions> _externalApi;

        public AppSettings(
            IHostEnvironment hostEnvironment,
            IConfiguration configuration,
            IOptionsMonitor<DatabaseOptions> database,
            IOptionsMonitor<BlobStorageOptions> blobStorage,
            IOptionsMonitor<SendGridOptions> sendGrid,
            IOptionsMonitor<JwtOptions> jwt,
            IOptionsMonitor<ExternalApiOptions> externalApi)
        {
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _database = database;
            _blobStorage = blobStorage;
            _sendGrid = sendGrid;
            _jwt = jwt;
            _externalApi = externalApi;
        }

        public string Environment => _hostEnvironment.EnvironmentName;

        public int DbCommandTimeoutSeconds => _database.CurrentValue.CommandTimeout;

        public string BlobContainerName => _blobStorage.CurrentValue.ContainerName;

        public string SendGridFromEmail => _sendGrid.CurrentValue.FromEmail;

        public string JwtIssuer => _jwt.CurrentValue.Issuer;
        public string JwtAudience => _jwt.CurrentValue.Audience;
        public int JwtExpiryMinutes => _jwt.CurrentValue.ExpiryMinutes;

        public string ExternalApiBaseUrl => _externalApi.CurrentValue.BaseUrl;

        public bool FeatureFlag(string key, bool defaultValue = false)
            => _configuration.GetValue($"FeatureFlags:{key}", defaultValue);
    }
}
