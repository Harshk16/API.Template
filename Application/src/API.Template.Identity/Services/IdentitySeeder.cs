using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Services
{
    public static class IdentitySeeder
    {
        // Central place to define default roles — add more here as your
        // app grows (e.g. "Manager", "Support"). Anyone cloning this
        // boilerplate edits just this list.
        private static readonly string[] DefaultRoles = { "Admin", "User" };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // This runs AFTER builder.Build(), so it's safe to resolve
            // scoped/singleton services here — unlike the AddXServices
            // extension methods, which run before the container exists.
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            foreach (var roleName in DefaultRoles)
            {
                var exists = await roleManager.RoleExistsAsync(roleName);
                if (!exists)
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
                    }
                }
            }
        }
    }
}
