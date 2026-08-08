using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Validation
{
    public sealed class BlobStorageOptionsValidator : IValidateOptions<BlobStorageOptions>
    {
        public ValidateOptionsResult Validate(string? name, BlobStorageOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                return ValidateOptionsResult.Fail(
                    "BlobStorage:ConnectionString is required. Check Key Vault access or user-secrets.");

            if (string.IsNullOrWhiteSpace(options.ContainerName))
                return ValidateOptionsResult.Fail("BlobStorage:ContainerName is required.");

            return ValidateOptionsResult.Success;
        }
    }
}
