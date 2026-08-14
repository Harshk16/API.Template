using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Entities
{
    public sealed class AuditLog
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;   // stored as string — works regardless of key type
        public string Action { get; set; } = string.Empty;      // "Create", "Update", "Delete"
        public Guid? ChangedByUserId { get; set; }
        public DateTime ChangedUtc { get; set; }
    }
}
