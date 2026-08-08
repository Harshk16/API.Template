using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Validation
{
    public sealed class ExternalApiOptionsValidator : IValidateOptions<ExternalApiOptions>
    {
        public ValidateOptionsResult Validate(string? name, ExternalApiOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                return ValidateOptionsResult.Fail(
                    "ExternalApi:ApiKey is required. Check Key Vault access or user-secrets.");

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
                return ValidateOptionsResult.Fail("ExternalApi:BaseUrl is required.");

            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
                return ValidateOptionsResult.Fail("ExternalApi:BaseUrl must be a valid absolute URL.");

            if (options.TimeoutSeconds is < 1 or > 120)
                return ValidateOptionsResult.Fail("ExternalApi:TimeoutSeconds must be between 1 and 120.");

            return ValidateOptionsResult.Success;
        }
    }
}
