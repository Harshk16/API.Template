using API.Template.Application.Interfaces;
using API.Template.Infrastructure.Configuration.Adapters;
using API.Template.Infrastructure.Configuration.Options;
using API.Template.Infrastructure.Configuration.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.DI
{
    public static partial class Extensions
    {
        public static IServiceCollection AddAllConfigurationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseConfiguration(configuration);
            services.AddSendGridConfiguration(configuration);
            services.AddBlobStorageConfiguration(configuration);
            services.AddJwtConfiguration(configuration);
            services.AddExternalApiConfiguration(configuration);
            services.AddAzureKeyVaultConfiguration(configuration);

            services.AddSingleton<IEnvironmentContext, EnvironmentContext>();
            services.AddSingleton<IKeys, AppKeys>();
            services.AddSingleton<ISettings, AppSettings>();

            return services;
        }

        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();
            return services;
        }

        public static IServiceCollection AddSendGridConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<SendGridOptions>()
                .Bind(configuration.GetSection(SendGridOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<IValidateOptions<SendGridOptions>, SendGridOptionsValidator>();
            return services;
        }

        public static IServiceCollection AddBlobStorageConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageOptions>()
                .Bind(configuration.GetSection(BlobStorageOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<IValidateOptions<BlobStorageOptions>, BlobStorageOptionsValidator>();
            return services;
        }

        public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
            return services;
        }

        public static IServiceCollection AddExternalApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ExternalApiOptions>()
                .Bind(configuration.GetSection(ExternalApiOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<IValidateOptions<ExternalApiOptions>, ExternalApiOptionsValidator>();
            return services;
        }

        public static IServiceCollection AddAzureKeyVaultConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<AzureKeyVaultOptions>()
                .Bind(configuration.GetSection(AzureKeyVaultOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<IValidateOptions<AzureKeyVaultOptions>, AzureKeyVaultOptionsValidator>();
            return services;
        }
    }
}
