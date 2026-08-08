using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Adapters
{
    /// <summary>
    /// Maps Key Vault secret names to IConfiguration keys. Key Vault
    /// secret names cannot contain ':' so '--' is used as the hierarchy
    /// separator in the vault — e.g. the vault secret named
    /// "Database--ConnectionString" becomes config key
    /// "Database:ConnectionString", which is exactly what
    /// DatabaseOptions.ConnectionString binds from.
    ///
    /// Optional prefix lets one shared vault serve multiple applications
    /// safely — only secrets starting with the prefix are loaded, and the
    /// prefix is stripped before the '--' → ':' mapping.
    /// </summary>
    public sealed class AppKeyVaultSecretManager : KeyVaultSecretManager
    {
        private readonly string? _prefix;

        public AppKeyVaultSecretManager(string? prefix = null)
        {
            _prefix = prefix;
        }

        public override bool Load(SecretProperties secret)
        {
            if (string.IsNullOrEmpty(_prefix))
                return true;

            return secret.Name.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            var name = string.IsNullOrEmpty(_prefix)
                ? secret.Name
                : secret.Name[_prefix.Length..];

            return name.Replace("--", ":");
        }
    }
}
