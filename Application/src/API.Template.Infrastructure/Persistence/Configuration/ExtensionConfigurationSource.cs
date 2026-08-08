using API.Template.Infrastructure.Configuration.Adapters;
using API.Template.Infrastructure.Persistence.Configuration;
// ============================================================
// File: Infrastructure/Persistence/Configuration/ConfigurationSourceExtensions.cs
// (renamed from ConfigurationBuilderExtensions.cs — now holds every
//  IConfigurationBuilder extension, not just the DB one)
// ============================================================
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configuration;

/// <summary>
/// Extensions on IConfigurationBuilder — i.e. config SOURCES, which
/// must be wired up before builder.Build() runs. These are only ever
/// called from Program.cs, in order, before any IServiceCollection
/// registration happens. Distinct from ServiceCollectionExtensions
/// (Step 8), which registers services against an already-built
/// IConfiguration.
/// </summary>
public static class ConfigurationSourceExtensions
{
    /// <summary>
    /// Adds Azure Key Vault as a config source, environment-aware:
    ///   - "Local": optional. Adds User Secrets instead (Local is a
    ///     genuinely distinct environment name, not ASP.NET Core's
    ///     built-in "Development", so the SDK's auto-add doesn't fire
    ///     — this call does it explicitly). Key Vault only wired up if
    ///     a VaultUri was explicitly set for Local.
    ///   - Dev / QA / Stag / UAT / Prod: MANDATORY. Throws at startup
    ///     if VaultUri is missing, rather than booting with empty
    ///     secrets and failing on the first request.
    /// </summary>
    public static IConfigurationBuilder AddAppKeyVault(
        this IConfigurationBuilder builder,
        IHostEnvironment environment)
    {
        var vaultUri = builder.Build()["AzureKeyVault:VaultUri"];

        //if (environment.IsEnvironment("Local"))
        //{
        //    if (!string.IsNullOrWhiteSpace(vaultUri))
        //    {
        //        builder.AddAzureKeyVault(
        //            new Uri(vaultUri),
        //            new DefaultAzureCredential(),
        //            new AppKeyVaultSecretManager());
        //    }

        //    return builder;
        //}

        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            throw new InvalidOperationException(
                $"AzureKeyVault:VaultUri is required in the '{environment.EnvironmentName}' environment but was not found. " +
                $"Check appsettings.{environment.EnvironmentName}.json or the AzureKeyVault__VaultUri environment variable.");
        }

        builder.AddAzureKeyVault(
            new Uri(vaultUri),
            new DefaultAzureCredential(),
            new AppKeyVaultSecretManager());

        return builder;
    }

    //PART - 2 - Load Base on Configuration Source
    // ============================================================
    // File: Infrastructure/Persistence/Configuration/ConfigurationSourceExtensions.cs
    // (AddAppKeyVault — revised to be config-driven, not environment-hardcoded)
    // ============================================================
    //public static IConfigurationBuilder AddAppKeyVault(
    //    this IConfigurationBuilder builder,
    //    IHostEnvironment environment)
    //{
    //    var partialConfig = builder.Build();

    //    var useKeyVault = partialConfig.GetValue("Configuration:UseKeyVault", defaultValue: true);
    //    if (!useKeyVault)
    //    {
    //        // Explicitly opted out — small project, client doesn't want Azure
    //        // dependency, whatever the reason. Secrets must then come from
    //        // User Secrets (Local) or straight environment variables instead.
    //        return builder;
    //    }

    //    var vaultUri = partialConfig["AzureKeyVault:VaultUri"];
    //    if (string.IsNullOrWhiteSpace(vaultUri))
    //    {
    //        throw new InvalidOperationException(
    //            $"Configuration:UseKeyVault is true but AzureKeyVault:VaultUri is missing in " +
    //            $"'{environment.EnvironmentName}'. Set VaultUri or set Configuration:UseKeyVault to false.");
    //    }

    //    builder.AddAzureKeyVault(
    //        new Uri(vaultUri),
    //        new DefaultAzureCredential(),
    //        new AppKeyVaultSecretManager());

    //    return builder;
    //}

    /// <summary>
    /// Adds dbo.Settings as a configuration source. Resolves its own
    /// BootstrapConnection and ReloadIntervalSeconds internally from
    /// whatever was already loaded (must be called AFTER AddAppKeyVault
    /// / user secrets). Throws if BootstrapConnection can't be resolved.
    /// </summary>
    public static IConfigurationBuilder AddDatabaseSettings(this IConfigurationBuilder builder)
    {
        var partialConfig = builder.Build();

        var bootstrapConnection = partialConfig.GetConnectionString("BootstrapConnection");
        if (string.IsNullOrWhiteSpace(bootstrapConnection))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:BootstrapConnection could not be resolved. " +
                "Check Key Vault access (non-Local) or user-secrets (Local).");
        }

        var reloadSeconds = partialConfig.GetValue("AppRuntimeSettings:ReloadIntervalSeconds", defaultValue: 300);

        return builder.Add(new DatabaseSettingsConfigurationSource(
            bootstrapConnection,
            TimeSpan.FromSeconds(reloadSeconds)));
    }

    //PART - 2 - Load Base on Configuration Source
    //public static IConfigurationBuilder AddDatabaseSettings(this IConfigurationBuilder builder)
    //{
    //    var partialConfig = builder.Build();

    //    var useDatabaseSettings = partialConfig.GetValue("Configuration:UseDatabaseSettings", defaultValue: true);
    //    if (!useDatabaseSettings)
    //    {
    //        // Small project doesn't need runtime-tunable settings — skip
    //        // entirely, no BootstrapConnection required either.
    //        return builder;
    //    }

    //    var bootstrapConnection = partialConfig.GetConnectionString("BootstrapConnection");
    //    if (string.IsNullOrWhiteSpace(bootstrapConnection))
    //    {
    //        throw new InvalidOperationException(
    //            "Configuration:UseDatabaseSettings is true but ConnectionStrings:BootstrapConnection " +
    //            "could not be resolved. Set it, or set Configuration:UseDatabaseSettings to false.");
    //    }

    //    var reloadSeconds = partialConfig.GetValue("AppRuntimeSettings:ReloadIntervalSeconds", defaultValue: 300);

    //    return builder.Add(new DatabaseSettingsConfigurationSource(
    //        bootstrapConnection,
    //        TimeSpan.FromSeconds(reloadSeconds)));
    //}
}