using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Interfaces
{
    /// <summary>
    /// Issues and validates JWTs. Uses IKeys.JwtSigningKey and
    /// ISettings.JwtIssuer/JwtAudience/JwtExpiryMinutes internally —
    /// callers never see those directly, just a token in, claims out.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Roles are looked up internally via IIdentityService — callers
        /// don't need to fetch and pass them in separately.
        /// </summary>
        Task<string> GenerateTokenAsync(Guid userId, string email, CancellationToken ct = default);

        // New — generates an access token + refresh token pair together (used at login)
        Task<(string AccessToken, string RefreshToken)> GenerateTokenPairAsync(
            Guid userId, string email, CancellationToken ct = default);

        // New — validates a refresh token, rotates it, and returns a fresh pair
        Task<(bool Succeeded, string? AccessToken, string? RefreshToken, string? Error)> RefreshAsync(
            string refreshToken, CancellationToken ct = default);

        // New — used on logout / explicit session termination
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}
