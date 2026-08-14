using API.Template.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Application
{
    //public static partial class Extension
    //{
    //    public static IServiceCollection AddAllApplicationServices(this IServiceCollection services)
    //    {
    //        services.AddApplicationService();
    //        return services;
    //    }

    //    public static IServiceCollection AddApplicationService(this IServiceCollection services)
    //    {
    //        // 1 Use typeof to get assembly from Application layer - type-safe & refactor-friendly
    //        services.AddMediatR(cfg =>
    //        {
    //            cfg.RegisterServicesFromAssembly(typeof(IApplicationMarker).Assembly);
    //        });

    //        // 2 Explicitly load the Application assembly
    //        //var assemblyName = "API.Template.Application";
    //        //var assembly = AppDomain.CurrentDomain.Load(assemblyName);

    //        //services.AddMediatR(cfg =>
    //        //{
    //        //    cfg.RegisterServicesFromAssembly(assembly);
    //        //});

    //        // 3 Use shared scanner from Core layer
    //        //var assemblies = AppAssemblyScanner.DiscoverAssemblies("API.Template");

    //        //services.AddMediatR(cfg =>
    //        //{
    //        //    cfg.RegisterServicesFromAssemblies(assemblies.ToArray());
    //        //});

    //        return services;
    //    }
    //}
}
