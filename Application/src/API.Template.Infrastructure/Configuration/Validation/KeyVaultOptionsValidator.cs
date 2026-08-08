using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Validation
{
    public sealed class KeyVaultOptionsValidator : IValidateOptions<AzureKeyVaultOptions>
    {
        public ValidateOptionsResult Validate(string? name, AzureKeyVaultOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.VaultUri))
            {
                return ValidateOptionsResult.Fail(
                    "KeyVault:VaultUri is required.");
            }

            if (!Uri.TryCreate(options.VaultUri, UriKind.Absolute, out _))
            {
                return ValidateOptionsResult.Fail(
                    "KeyVault:VaultUri is not a valid absolute URI.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
