using API.Template.Application.Common.Models;
using API.Template.Application.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Interfaces
{

    /// <summary>
    /// Abstraction over user account operations. No Identity types appear
    /// here — Application/Domain never reference ApplicationUser,
    /// UserManager<T>, or anything from API.Template.Identity directly.
    /// </summary>
    public interface IIdentityService
    {
        Task<(bool Succeeded, Guid? UserId, IEnumerable<string> Errors)> CreateUserAsync(
            string email, string password, string firstName, string lastName, CancellationToken ct = default);

        Task<SignInResultDto> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);

        Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);

        Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken ct = default);

        Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);

        Task<string> GetSecurityStampAsync(Guid userId, CancellationToken ct = default);

        Task<(bool Succeeded, IEnumerable<string> Errors)> AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default);

        // New — generates a password reset token for a given email.
        // Returns null if the email doesn't exist — caller must NOT reveal
        // this distinction to the client (enumeration protection).
        Task<(Guid? UserId, string? Token)> GeneratePasswordResetTokenAsync(
            string email, CancellationToken ct = default);

        // New — validates the reset token and sets the new password.
        Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(
            string email, string token, string newPassword, CancellationToken ct = default);

        // New — for an already-authenticated user changing their own password.
        // Verifies currentPassword before allowing the change (extra confirmation
        // beyond just holding a valid token).
        Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(
            Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);

    }
}
