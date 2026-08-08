using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Row shape for dbo.AppSettings. Non-secret, operationally tunable
    /// values ONLY — the provider that reads this table rejects rows
    /// whose Key looks like a secret. CreatedBy/ModifiedBy are free-text
    /// (username/email), not a FK — this table is typically edited
    /// directly via SQL or an internal admin tool, not through normal
    /// app-user auth.
    /// </summary>
    //public sealed class AppSettingEntity
    //{
    //    public string Key { get; set; } = string.Empty;
    //    public string? Value { get; set; }

    //    public DateTime CreatedOn { get; set; }
    //    public string? CreatedBy { get; set; }

    //    public DateTime? ModifiedOn { get; set; }
    //    public string? ModifiedBy { get; set; }
    //}
}
