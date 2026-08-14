using API.Template.Application.Common.Models;
using API.Template.Application.Interfaces;
using API.Template.Application.Interfaces.Models;
using API.Template.Identity.Entities;
using API.Template.Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Services
{
    /// <summary>
    /// Implements IIdentityService by wrapping UserManager<ApplicationUser>.
    /// This is the ONLY place in the codebase UserManager/ApplicationUser
    /// should be referenced directly — everything else goes through the
    /// interface.
    /// </summary>
    internal sealed class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Succeeded, Guid? UserId, IEnumerable<string> Errors)> CreateUserAsync(
            string email, string password, string firstName, string lastName, CancellationToken ct = default)
        {

            // IgnoreQueryFilters so this sees soft-deleted rows too — Identity's own
            // RequireUniqueEmail check won't, since it queries through the filtered set.
            var existing = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant(), ct);

            if (existing is not null)
                return (false, null, new[] { "Email is already registered." });

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return (false, null, result.Errors.Select(e => e.Description));

            return (true, user.Id, Enumerable.Empty<string>());
        }

        public async Task<SignInResultDto> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Deliberately generic failure for "no such user" vs "wrong password" —
            // avoids leaking which emails are registered (user enumeration).
            if (user is null)
                return new SignInResultDto(SignInStatus.InvalidCredentials, null);

            if (!user.IsActive)
                return new SignInResultDto(SignInStatus.NotAllowed, null);

            // CheckPasswordSignInAsync (not UserManager.CheckPasswordAsync) is
            // what actually tracks failed attempts and enforces lockout —
            // lockoutOnFailure: true increments AccessFailedCount on a wrong
            // password and locks the account once MaxFailedAccessAttempts is hit.
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return new SignInResultDto(SignInStatus.LockedOut, null);

            if (!result.Succeeded)
                return new SignInResultDto(SignInStatus.InvalidCredentials, null);

            return new SignInResultDto(SignInStatus.Succeeded, ToDto(user));
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user is null ? null : ToDto(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user is null ? null : ToDto(user);
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return Array.Empty<string>();

            return await _userManager.GetRolesAsync(user);
        }

        private static UserDto ToDto(ApplicationUser user) =>
            new(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, user.IsActive);

        public async Task<string> GetSecurityStampAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return string.Empty;

            return await _userManager.GetSecurityStampAsync(user);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> AssignRoleAsync(
    Guid userId, string roleName, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return (false, new[] { "User not found." });

            var alreadyInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (alreadyInRole)
                return (true, Enumerable.Empty<string>()); // idempotent — no-op if already assigned

            var result = await _userManager.AddToRoleAsync(user, roleName);

            return result.Succeeded
                ? (true, Enumerable.Empty<string>())
                : (false, result.Errors.Select(e => e.Description));
        }

        public async Task<(Guid? UserId, string? Token)> GeneratePasswordResetTokenAsync(
    string email, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Deliberately no distinction between "user not found" and any other
            // failure here — same enumeration-protection principle as
            // ValidateCredentialsAsync. The CALLER (controller) must always return
            // a generic success message regardless of this result.
            if (user is null)
                return (null, null);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return (user.Id, token);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(
            string email, string token, string newPassword, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return (false, new[] { "Invalid request." }); // generic — don't reveal user existence

            // ResetPasswordAsync validates the token internally (checks signature,
            // expiry, and that it was issued for THIS user) and, on success,
            // automatically rotates the user's SecurityStamp — which immediately
            // invalidates any existing JWTs/refresh tokens tied to the old password,
            // via the OnTokenValidated check from Fix 3/4.
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            return result.Succeeded
                ? (true, Enumerable.Empty<string>())
                : (false, result.Errors.Select(e => e.Description));
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(
    Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
                return (false, new[] { "User not found." });

            // ChangePasswordAsync verifies currentPassword internally before applying
            // newPassword — fails with an "IncorrectPassword" error if it doesn't match.
            // Like ResetPasswordAsync, this also automatically rotates the user's
            // SecurityStamp on success, invalidating all other existing tokens.
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            return result.Succeeded
                ? (true, Enumerable.Empty<string>())
                : (false, result.Errors.Select(e => e.Description));
        }
    }
}
