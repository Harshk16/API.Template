using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Validation
{
    public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
    {
        // JWT signing keys need real entropy — HMAC-SHA256 wants at least
        // 256 bits (32 bytes). A short/weak key is a genuine security bug,
        // not just a missing-value bug, so it's checked here specifically.
        private const int MinimumSigningKeyLength = 32;

        public ValidateOptionsResult Validate(string? name, JwtOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.SigningKey))
                return ValidateOptionsResult.Fail(
                    "Jwt:SigningKey is required. Check Key Vault access or user-secrets.");

            if (options.SigningKey.Length < MinimumSigningKeyLength)
                return ValidateOptionsResult.Fail(
                    $"Jwt:SigningKey must be at least {MinimumSigningKeyLength} characters for HMAC-SHA256.");

            if (string.IsNullOrWhiteSpace(options.Issuer))
                return ValidateOptionsResult.Fail("Jwt:Issuer is required.");

            if (string.IsNullOrWhiteSpace(options.Audience))
                return ValidateOptionsResult.Fail("Jwt:Audience is required.");

            if (options.ExpiryMinutes is < 1 or > 1440)
                return ValidateOptionsResult.Fail("Jwt:ExpiryMinutes must be between 1 and 1440.");

            return ValidateOptionsResult.Success;
        }
    }
}
