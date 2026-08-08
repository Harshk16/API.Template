using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Template.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Registers the DB-backed settings source. Uses a raw ADO.NET
    /// connection (not EF/DbContext) deliberately: this runs during host
    /// configuration build, before the DI container — and therefore
    /// before AppDbContext — exists.
    /// </summary>
    public sealed class DatabaseSettingsConfigurationSource : IConfigurationSource
    {
        private readonly string _connectionString;
        private readonly TimeSpan _reloadInterval;

        public DatabaseSettingsConfigurationSource(string connectionString, TimeSpan reloadInterval)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _reloadInterval = reloadInterval;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new DatabaseSettingsConfigurationProvider(_connectionString, _reloadInterval);
    }

    /// <summary>
    /// Thread-safe configuration provider backed by dbo.Settings.
    ///
    /// WHAT THIS DOES NOT DO: fetch connection strings or any other
    /// secret. That is deliberate — this provider actively REJECTS rows
    /// whose Key looks like a secret (see SecretKeyMarkers) rather than
    /// trusting whoever inserts rows into the table to know the rule.
    ///
    /// THREAD SAFETY:
    ///  - Data[] (inherited from ConfigurationProvider) is only ever
    ///    replaced wholesale, never mutated in place, under a lock.
    ///  - A background Timer polls on a separate thread and swaps the
    ///    dictionary atomically, then raises OnReload() so
    ///    IOptionsMonitor&lt;T&gt; subscribers and change tokens fire correctly.
    ///  - Initial Load() is synchronous (GetAwaiter().GetResult()) because
    ///    IConfigurationProvider.Load() has no async signature and the
    ///    host must not start serving requests before config is ready.
    /// </summary>
    public sealed class DatabaseSettingsConfigurationProvider : ConfigurationProvider, IDisposable
    {
        // Any key containing one of these (case-insensitive) is rejected —
        // it belongs in Key Vault, not this table.
        private static readonly string[] SecretKeyMarkers =
        {
        "connectionstring", "apikey", "api-key", "secret", "password", "clientsecret", "accesskey", "token", "signingkey"
    };

        private readonly string _connectionString;
        private readonly TimeSpan _reloadInterval;
        private readonly object _reloadLock = new();
        private Timer? _timer;
        private volatile bool _disposed;

        public DatabaseSettingsConfigurationProvider(string connectionString, TimeSpan reloadInterval)
        {
            _connectionString = connectionString;
            _reloadInterval = reloadInterval;
        }

        public override void Load()
        {
            // Synchronous, deliberate: startup must not proceed with partial/missing config.
            var data = LoadFromDatabaseAsync().GetAwaiter().GetResult();

            lock (_reloadLock)
            {
                Data = data;
            }

            _timer ??= new Timer(
                callback: _ => ReloadSafe(),
                state: null,
                dueTime: _reloadInterval,
                period: _reloadInterval);
        }

        private void ReloadSafe()
        {
            if (_disposed) return;

            try
            {
                var freshData = LoadFromDatabaseAsync().GetAwaiter().GetResult();

                lock (_reloadLock)
                {
                    Data = freshData;
                }

                OnReload();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"[DatabaseSettingsConfigurationProvider] Reload failed, keeping last-known-good values: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, string?>> LoadFromDatabaseAsync()
        {
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT [Key], [Value] FROM dbo.Settings";
            command.CommandTimeout = 15;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var key = reader.GetString(0);
                var value = reader.IsDBNull(1) ? null : reader.GetString(1);

                if (SecretKeyMarkers.Any(marker => key.Contains(marker, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.Error.WriteLine(
                        $"[DatabaseSettingsConfigurationProvider] Rejected key '{key}' from dbo.Settings — " +
                        "looks like a secret. Secrets must be stored in Azure Key Vault, not this table.");
                    continue;
                }

                result[key] = value;
            }

            return result;
        }

        public void Dispose()
        {
            _disposed = true;
            _timer?.Dispose();
        }
    }
}
