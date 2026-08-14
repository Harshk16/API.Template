using Microsoft.AspNetCore.Identity;

namespace API.Template.Identity.Entities
{
    /// <summary>
    /// Baseline user entity for the template. IdentityUser<Guid> already
    /// provides: Id, UserName, Email, EmailConfirmed, PasswordHash,
    /// PhoneNumber, PhoneNumberConfirmed, SecurityStamp, ConcurrencyStamp,
    /// LockoutEnd, LockoutEnabled, AccessFailedCount, TwoFactorEnabled.
    ///
    /// Audit/soft-delete fields use raw Guid? (no navigation property) —
    /// same pattern as business entities referencing users: avoids a
    /// self-referencing EF navigation, which adds real complexity for
    /// little benefit here.
    ///
    /// Deliberately NOT included (add per-project when actually needed):
    /// OrganizationId (multi-tenancy), multi-email/phone history,
    /// DateOfBirth/Gender (domain-specific), a flexible JSON column.
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Computed, not stored — avoids a second source of truth that could
        // drift out of sync with FirstName/LastName (the exact problem
        // flagged with your primary_email vs. user_emails duplication).
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string? AvatarUrl { get; set; }

        // IsActive: admin-disabled account (reversible) — user still exists, just can't sign in.
        // IsDeleted: soft-deleted (query-filtered out entirely) — treated as if the user doesn't exist.
        public bool IsActive { get; set; } = true;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public Guid? CreatedByUserId { get; set; }

        public DateTime? ModifiedUtc { get; set; }
        public Guid? ModifiedByUserId { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedUtc { get; set; }
        public Guid? DeletedByUserId { get; set; }
    }
}
