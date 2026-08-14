using API.Template.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Contexts
{
    /// <summary>
    /// Self-contained EF context for Identity — separate from AppDbContext
    /// (business entities), by design: keeps Identity swappable/pluggable
    /// as its own module, independent migration history. Same physical
    /// database as AppDbContext, different DbContext.
    ///
    /// Uses Guid as the key type throughout (users AND roles), matching
    /// ApplicationUser : IdentityUser<Guid>.
    /// </summary>
    public sealed class AppIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // IMPORTANT: must run first — sets up all the Identity table mappings

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.AvatarUrl).HasMaxLength(500);

                entity.Ignore(u => u.FullName);

                // Single combined filter — soft-deleted users are excluded from
                // every query. IsActive is intentionally NOT part of this filter:
                // an inactive (admin-disabled) user should still be findable by
                // FindByIdAsync/FindByEmailAsync (e.g. so an admin can look them
                // up and reactivate them) — ValidateCredentialsAsync already
                // checks IsActive separately as a sign-in business rule, which is
                // the correct place for that check, not the query filter.
                entity.HasQueryFilter(u => !u.IsDeleted);
            });
        }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    }
}
