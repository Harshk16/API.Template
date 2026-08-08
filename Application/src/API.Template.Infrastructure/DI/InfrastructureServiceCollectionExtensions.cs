using API.Template.Infrastructure.Configuration.Extensions;
using API.Template.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.DI
{
    //public static class InfrastructureServiceCollectionExtensions
    //{
    //    public static IServiceCollection AddInfrastructure(
    //        this IServiceCollection services,
    //        IConfiguration configuration,
    //        IHostEnvironment environment)
    //    {
    //        services.AddDatabaseConfiguration(configuration);

    //        //if (environment.UseKeyVault())
    //        //{
    //        //    services.AddKeyVaultConfiguration(configuration);
    //        //}

    //        var options = configuration
    //           .GetSection(ConfigurationOptions.SectionName)
    //           .Get<ConfigurationOptions>() ?? new();

    //        if (options.UseKeyVault)
    //        {
    //            services.AddKeyVaultConfiguration(configuration);
    //        }

    //        // Future
    //        // services.AddApplicationDbContext(configuration);
    //        // services.AddRedis(configuration);
    //        // services.AddBlobStorage(configuration);

    //        return services;
    //    }
    //}
}
