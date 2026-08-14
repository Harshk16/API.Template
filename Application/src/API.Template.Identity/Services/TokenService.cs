using API.Template.Application.Interfaces;
using API.Template.Identity.Contexts;
using API.Template.Identity.Entities;
using API.Template.Identity.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Services
{
    /// <summary>
    /// Issues JWTs. IKeys/ISettings supply the signing key, issuer,
    /// audience, and expiry — this class never reads config directly,
    /// same rule as everywhere else in the codebase.
    /// </summary>
    internal sealed class TokenService : ITokenService
    {
        private readonly IKeys _keys;
        private readonly ISettings _settings;
        private readonly IIdentityService _identityService;
        private readonly AppIdentityDbContext _dbContext; // ← new dependency

        public TokenService(
            IKeys keys,
            ISettings settings,
            IIdentityService identityService,
            AppIdentityDbContext dbContext)
        {
            _keys = keys;
            _settings = settings;
            _identityService = identityService;
            _dbContext = dbContext;
        }

        public async Task<string> GenerateTokenAsync(Guid userId, string email, CancellationToken ct = default)
        {
            var roles = await _identityService.GetUserRolesAsync(userId, ct);
            var securityStamp = await _identityService.GetSecurityStampAsync(userId, ct);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("security_stamp", securityStamp)
        };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_keys.JwtSigningKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.JwtIssuer,
                audience: _settings.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.JwtExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokenPairAsync(
            Guid userId, string email, CancellationToken ct = default)
        {
            var accessToken = await GenerateTokenAsync(userId, email, ct);
            var refreshToken = await CreateAndStoreRefreshTokenAsync(userId, ct);

            return (accessToken, refreshToken);
        }

        public async Task<(bool Succeeded, string? AccessToken, string? RefreshToken, string? Error)> RefreshAsync(
            string refreshToken, CancellationToken ct = default)
        {
            var hash = TokenHasher.Hash(refreshToken);

            var existing = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

            if (existing is null)
                return (false, null, null, "Invalid refresh token.");

            // Reuse of an already-rotated (or revoked) token is a strong theft
            // signal — the legitimate holder should always have the LATEST
            // token in the chain, never a used one. Revoke the whole chain.
            if (!existing.IsActive)
            {
                await RevokeTokenChainAsync(existing.UserId, ct);
                return (false, null, null, "Refresh token has been revoked. Please log in again.");
            }

            var user = await _identityService.GetUserByIdAsync(existing.UserId, ct);
            if (user is null)
                return (false, null, null, "User not found.");

            // Rotation: kill the old token, issue a brand-new pair.
            var newRefreshToken = await CreateAndStoreRefreshTokenAsync(existing.UserId, ct);
            var newRefreshTokenHash = TokenHasher.Hash(newRefreshToken);

            var newRefreshTokenEntity = await _dbContext.RefreshTokens
                .FirstAsync(t => t.TokenHash == newRefreshTokenHash, ct);

            existing.RevokedUtc = DateTime.UtcNow;
            existing.ReplacedByTokenId = newRefreshTokenEntity.Id;

            await _dbContext.SaveChangesAsync(ct);

            var newAccessToken = await GenerateTokenAsync(existing.UserId, user.Email, ct);

            return (true, newAccessToken, newRefreshToken, null);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var hash = TokenHasher.Hash(refreshToken);

            var existing = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

            if (existing is not null && existing.RevokedUtc is null)
            {
                existing.RevokedUtc = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        private async Task<string> CreateAndStoreRefreshTokenAsync(Guid userId, CancellationToken ct)
        {
            // 256 bits of randomness — cryptographically secure, unlike Guid.NewGuid().
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = TokenHasher.Hash(rawToken),
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(_settings.JwtExpiryMinutes)
            };

            _dbContext.RefreshTokens.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            return rawToken; // raw value returned to caller — only the hash is persisted
        }

        private async Task RevokeTokenChainAsync(Guid userId, CancellationToken ct)
        {
            var activeTokens = await _dbContext.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedUtc == null)
                .ToListAsync(ct);

            foreach (var token in activeTokens)
                token.RevokedUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}

