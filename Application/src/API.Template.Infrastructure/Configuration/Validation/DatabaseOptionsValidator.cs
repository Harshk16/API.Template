using API.Template.Infrastructure.Configuration.Enums;
using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Validation
{
    public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
    {
        public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
        {
            // Connection String
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                return ValidateOptionsResult.Fail(
                    "Database:ConnectionString is required.");
            }

            // Database Provider
            if (string.IsNullOrWhiteSpace(options.Provider))
            {
                return ValidateOptionsResult.Fail(
                    "Database:Provider is required.");
            }

            if (!Enum.TryParse<DatabaseProvider>(
                    options.Provider,
                    ignoreCase: true,
                    out _))
            {
                return ValidateOptionsResult.Fail(
                    $"Database:Provider '{options.Provider}' is invalid. Supported values: {string.Join(", ", Enum.GetNames<DatabaseProvider>())}");
            }

            // Command Timeout
            if (options.CommandTimeout <= 0)
            {
                return ValidateOptionsResult.Fail(
                    "Database:CommandTimeout must be greater than zero.");
            }

            // Retry Count
            if (options.MaxRetryCount < 0)
            {
                return ValidateOptionsResult.Fail(
                    "Database:MaxRetryCount cannot be negative.");
            }

            // Retry Delay
            if (options.MaxRetryDelaySeconds <= 0)
            {
                return ValidateOptionsResult.Fail(
                    "Database:MaxRetryDelaySeconds must be greater than zero.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
