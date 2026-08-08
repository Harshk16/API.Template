using API.Template.Application.Interfaces;
using API.Template.Core.Extensions;
using API.Template.Infrastructure.Configuration.Options;
using API.Template.Infrastructure.Configuration.Validation;
using FluentValidation;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace API.Template.Infrastructure.DI
{
    public static partial class Extensions
    {
        //public static IServiceCollection AddAllApplicationServices(this IServiceCollection services)
        //{
        //    services.AddApplicationService();
        //    return services;
        //}

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<AppDbContext>((sp, options) =>
            //{
            //    var keys = sp.GetRequiredService<IKeys>();
            //    var settings = sp.GetRequiredService<ISettings>();

            //    options.UseSqlServer(keys.DatabaseConnectionString, sql =>
            //    {
            //        sql.CommandTimeout(settings.DbCommandTimeoutSeconds);
            //        sql.EnableRetryOnFailure(3);
            //    });
            //});

            //services.AddSingleton<IEmailService, EmailService>();
            //services.AddSingleton<IBlobService, BlobService>();

            return services;
        }
    }
}
