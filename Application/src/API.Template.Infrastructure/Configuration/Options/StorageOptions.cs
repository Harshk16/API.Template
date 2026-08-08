using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Binds from the "BlobStorage" config section. ConnectionString
    /// resolves from Key Vault / User Secrets only. ContainerName is
    /// non-secret and may come from appsettings.json or be overridden
    /// via the DB AppSettings table per environment.
    /// </summary>
    public sealed class BlobStorageOptions
    {
        public const string SectionName = "BlobStorage";

        public string ConnectionString { get; init; } = string.Empty;

        public string ContainerName { get; init; } = string.Empty;
    }
}
