using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Validation
{
    public sealed class SendGridOptionsValidator : IValidateOptions<SendGridOptions>
    {
        public ValidateOptionsResult Validate(string? name, SendGridOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                return ValidateOptionsResult.Fail(
                    "SendGrid:ApiKey is required. Check Key Vault access or user-secrets.");

            if (string.IsNullOrWhiteSpace(options.FromEmail))
                return ValidateOptionsResult.Fail("SendGrid:FromEmail is required.");

            if (!options.FromEmail.Contains('@'))
                return ValidateOptionsResult.Fail("SendGrid:FromEmail is not a valid email address.");

            if (string.IsNullOrWhiteSpace(options.FromName))
                return ValidateOptionsResult.Fail("SendGrid:FromName is required.");

            return ValidateOptionsResult.Success;
        }
    }
}
