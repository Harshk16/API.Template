using API.Template.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Adapters
{
    /// <summary>
    /// Implements IEnvironmentContext by wrapping ASP.NET Core's
    /// IHostEnvironment — one source of truth (ASPNETCORE_ENVIRONMENT),
    /// no parallel custom enum to keep in sync (unlike Doc 1's
    /// eEnvironment). Registered as a singleton; the environment never
    /// changes during the app's lifetime, so there's no need for this to
    /// react to config reloads the way AppKeys/AppSettings do.
    /// </summary>
    internal sealed class EnvironmentContext : IEnvironmentContext
    {
        private readonly IHostEnvironment _hostEnvironment;

        public EnvironmentContext(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public string Name => _hostEnvironment.EnvironmentName;

        public bool IsDevelopment => _hostEnvironment.IsDevelopment();

        //public bool IsQA => _hostEnvironment.IsDevelopment();

        public bool IsStaging => _hostEnvironment.IsStaging();

        public bool IsProduction => _hostEnvironment.IsProduction();
    }
}
