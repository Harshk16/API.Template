using API.Template.Infrastructure.Configuration.Options;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        //    public static IConfigurationBuilder AddEnterpriseConfiguration(
        //        this IConfigurationBuilder configuration)
        //    {
        //        // Build current configuration
        //        var builtConfiguration = configuration.Build();

        //        // Read Key Vault URI from appsettings
        //        var keyVaultUri = builtConfiguration["KeyVault:VaultUri"];

        //        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        //        {
        //            configuration.AddAzureKeyVault(
        //                new Uri(keyVaultUri),
        //                new DefaultAzureCredential());
        //        }

        //        return configuration;
        //    }
        //}

        //public static class ConfigurationBuilderExtensions
        //{
        //    public static IConfigurationBuilder AddEnterpriseConfiguration(
        //        this IConfigurationBuilder configuration,
        //        IHostEnvironment environment)
        //    {
        //        var builtConfiguration = configuration.Build();

        //        var keyVaultUri = builtConfiguration["KeyVault:VaultUri"];

        //        // Local/Development
        //        if (environment.IsDevelopment() ||
        //            environment.IsEnvironment("Local"))
        //        {
        //            if (!string.IsNullOrWhiteSpace(keyVaultUri))
        //            {
        //                configuration.AddAzureKeyVault(
        //                    new Uri(keyVaultUri),
        //                    new DefaultAzureCredential());
        //            }
        //        }

        //        // QA/Staging/Production
        //        else
        //        {
        //            if (!string.IsNullOrWhiteSpace(keyVaultUri))
        //            {
        //                configuration.AddAzureKeyVault(
        //                    new Uri(keyVaultUri),
        //                    new DefaultAzureCredential());
        //            }
        //        }

        //        return configuration;
        //    }
        //}


        //public static class ConfigurationBuilderExtensions
        //{
        //    public static IConfigurationBuilder AddEnterpriseConfiguration(
        //        this IConfigurationBuilder configuration,
        //        IHostEnvironment environment,
        //        Assembly startupAssembly)
        //    {
        //        // Local environment
        //        if (environment.IsEnvironment("Local"))
        //        {
        //            configuration.AddUserSecrets(startupAssembly);
        //        }

        //        return configuration;
        //    }
        //}

        //public static class ConfigurationBuilderExtensions
        //{
        //    public static IConfigurationBuilder AddEnterpriseConfiguration(
        //        this IConfigurationBuilder configuration,
        //        IHostEnvironment environment,
        //        Assembly startupAssembly)
        //    {
        //        if (environment.UseUserSecrets())
        //        {
        //            configuration.AddUserSecrets(startupAssembly);
        //        }

        //        if (environment.UseKeyVault())
        //        {
        //            var builtConfiguration = configuration.Build();

        //            var keyVaultUri = builtConfiguration["KeyVault:VaultUri"];

        //            if (!string.IsNullOrWhiteSpace(keyVaultUri))
        //            {
        //                configuration.AddAzureKeyVault(
        //                    new Uri(keyVaultUri),
        //                    new DefaultAzureCredential());
        //            }
        //        }

        //        configuration.AddEnvironmentVariables();

        //        return configuration;
        //    }
        //}
    //    public static IConfigurationBuilder AddEnterpriseConfiguration(
    //this IConfigurationBuilder configuration,
    //ConfigurationOptions options)
    //    {
    //        if (options.UseKeyVault)
    //        {
    //            var builtConfiguration = configuration.Build();

    //            var keyVaultUri = builtConfiguration["KeyVault:VaultUri"];

    //            if (!string.IsNullOrWhiteSpace(keyVaultUri))
    //            {
    //                configuration.AddAzureKeyVault(
    //                    new Uri(keyVaultUri),
    //                    new DefaultAzureCredential());
    //            }
    //        }

    //        return configuration;
    //    }
    }
}
