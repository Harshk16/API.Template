using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Configuration.Extensions
{
    public static class EnvironmentExtensions
    {
        public static bool UseKeyVault(this IHostEnvironment environment)
        {
            //return !environment.IsEnvironment("Local");
            return !environment.UseUserSecrets();
        }

        public static bool UseUserSecrets(this IHostEnvironment environment)
        {
            return environment.IsEnvironment("Local");
        }
    }
}
