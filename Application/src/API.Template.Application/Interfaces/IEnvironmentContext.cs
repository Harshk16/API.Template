using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Application.Interfaces
{
    /// <summary>
    /// Wraps ASPNETCORE_ENVIRONMENT — one source of truth, no parallel
    /// custom enum to keep in sync (unlike Doc 1's eEnvironment).
    /// </summary>
    public interface IEnvironmentContext
    {
        string Name { get; }
        bool IsDevelopment { get; }
        bool IsStaging { get; }
        bool IsProduction { get; }
    }
}
