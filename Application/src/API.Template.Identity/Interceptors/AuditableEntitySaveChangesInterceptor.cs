using API.Template.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Interceptors
{
    public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditFields(DbContext? context)
        {
            if (context is null)
                return;

            var userId = _currentUserService.UserId;
            var now = DateTime.UtcNow;
            var auditLogs = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries<ApplicationUser>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedUtc = now;
                        entry.Entity.CreatedByUserId = userId;

                        auditLogs.Add(new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            TableName = nameof(ApplicationUser),
                            EntityId = entry.Entity.Id.ToString(),
                            Action = "Create",
                            ChangedByUserId = userId,
                            ChangedUtc = now
                        });
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedUtc = now;
                        entry.Entity.ModifiedByUserId = userId;

                        var isSoftDelete = entry.Entity.IsDeleted &&
                            entry.Property(nameof(ApplicationUser.IsDeleted)).IsModified;

                        if (isSoftDelete)
                        {
                            entry.Entity.DeletedUtc = now;
                            entry.Entity.DeletedByUserId = userId;
                        }

                        auditLogs.Add(new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            TableName = nameof(ApplicationUser),
                            EntityId = entry.Entity.Id.ToString(),
                            Action = isSoftDelete ? "Delete" : "Update",
                            ChangedByUserId = userId,
                            ChangedUtc = now
                        });
                        break;
                }
            }

            // Added directly to the DbContext so they're included in the same
            // SaveChanges call/transaction — audit rows and the actual data
            // change succeed or fail together, never out of sync.
            if (auditLogs.Count > 0)
                context.Set<AuditLog>().AddRange(auditLogs);
        }
    }
}
