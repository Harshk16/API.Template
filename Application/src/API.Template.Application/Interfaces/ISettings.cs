using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Application.Interfaces
{
    /// <summary>
    /// Non-secret, operational settings. Sourced from appsettings.json,
    /// appsettings.{Environment}.json, or the DB AppSettings table
    /// (which may itself override a json default per environment).
    /// Safe to log, safe to expose in diagnostics — never a credential.
    /// </summary>
    public interface ISettings
    {
        string Environment { get; }

        int DbCommandTimeoutSeconds { get; }

        string BlobContainerName { get; }

        string SendGridFromEmail { get; }

        string JwtIssuer { get; }

        string JwtAudience { get; }

        int JwtExpiryMinutes { get; }

        string ExternalApiBaseUrl { get; }

        /// <summary>
        /// Escape hatch for ad-hoc feature flags added directly to the DB
        /// AppSettings table without a code change / redeploy. Backed by
        /// IConfiguration, which the DB provider keeps live via its own
        /// reload timer.
        /// </summary>
        bool FeatureFlag(string key, bool defaultValue = false);
    }
}
