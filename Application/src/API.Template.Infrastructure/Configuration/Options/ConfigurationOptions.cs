using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Options
{
    public sealed class ConfigurationOptions
    {
        public const string SectionName = "Configuration";

        public bool UseUserSecrets { get; init; }

        public bool UseKeyVault { get; init; }

        public bool UseEnvironmentVariables { get; init; } = true;

        // Future
        public bool UseDatabaseSettings { get; init; }

        public bool UseAzureAppConfiguration { get; init; }
    }
}
