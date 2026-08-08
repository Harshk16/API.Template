using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Validation
{
    public sealed class AzureKeyVaultOptionsValidator : IValidateOptions<AzureKeyVaultOptions>
    {
        public ValidateOptionsResult Validate(string? name, AzureKeyVaultOptions options)
        {
            // Deliberately NOT requiring VaultUri here — it's legitimately
            // empty in Development when running off user-secrets only.
            // Program.cs enforces "required outside Development" separately,
            // since that's an environment-branching decision, not a pure
            // data-shape validation.
            if (!string.IsNullOrWhiteSpace(options.VaultUri) &&
                !Uri.TryCreate(options.VaultUri, UriKind.Absolute, out _))
            {
                return ValidateOptionsResult.Fail("AzureKeyVault:VaultUri, if set, must be a valid absolute URL.");
            }

            if (options.ReloadIntervalMinutes is < 1 or > 1440)
                return ValidateOptionsResult.Fail("AzureKeyVault:ReloadIntervalMinutes must be between 1 and 1440.");

            return ValidateOptionsResult.Success;
        }
    }
}
