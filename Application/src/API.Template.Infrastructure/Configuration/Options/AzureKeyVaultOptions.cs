using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Binds from the "AzureKeyVault" config section. VaultUri itself is
    /// NOT a secret — it's a non-sensitive endpoint address, safe to keep
    /// in appsettings.{Environment}.json (a different URI per environment
    /// is what actually makes Key Vault environment-aware — see Program.cs).
    /// </summary>
    public sealed class AzureKeyVaultOptions
    {
        public const string SectionName = "AzureKeyVault";

        public string VaultUri { get; init; } = string.Empty;

        public int ReloadIntervalMinutes { get; init; } = 15;
    }
}
