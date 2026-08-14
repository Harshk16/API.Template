using API.Template.Application.Interfaces;
using API.Template.Identity.Entities;
using API.Template.Identity.Contexts;
using API.Template.Identity.Interceptors;
using API.Template.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Template.Identity.Interfaces;

namespace API.Template.Identity
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers the entire Identity module: AppIdentityDbContext,
        /// ASP.NET Core Identity, JWT bearer authentication, and the two
        /// service implementations.
        ///
        /// Reads Database:ConnectionString / Jwt:SigningKey / Jwt:Issuer /
        /// Jwt:Audience directly from IConfiguration — NOT via IKeys/ISettings.
        ///
        /// WHY: IKeys and ISettings are DI-resolved services — getting an
        /// instance requires a built IServiceProvider, which only exists
        /// after var app = builder.Build() runs. AddIdentityServices runs
        /// BEFORE that point (during builder.Services.Add*(...) registration),
        /// so IKeys/ISettings cannot be resolved here — there is no container
        /// yet to resolve them from. IConfiguration, unlike IKeys/ISettings,
        /// is not DI-resolved — it's built separately and earlier
        /// (builder.Configuration), so it's safely readable at this phase.
        ///
        /// This is the SAME constraint AddDatabaseSettings already has, for
        /// the same reason. It is NOT valid to "fix" this by calling
        /// services.BuildServiceProvider() to force a container into
        /// existence early — that creates a second, throwaway container
        /// whose resolved instances can differ from the real app's. Reading
        /// IConfiguration directly is the correct, non-anti-pattern fix.
        ///
        /// Every consumer AFTER this startup phase (IdentityService,
        /// TokenService, MediatR handlers — anything resolved once the real
        /// app.Services provider exists) still only ever injects
        /// IKeys/ISettings, never IConfiguration directly. This method is
        /// the one deliberate, structurally-necessary exception.
        /// </summary>
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var connectionString = configuration["Database:ConnectionString"];
            var jwtSigningKey = configuration["Jwt:SigningKey"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];


            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Database:ConnectionString is required for AddIdentityServices.");
            if (string.IsNullOrWhiteSpace(jwtSigningKey))
                throw new InvalidOperationException("Jwt:SigningKey is required for AddIdentityServices.");
            if (string.IsNullOrWhiteSpace(jwtIssuer))
                throw new InvalidOperationException("Jwt:Issuer is required for AddIdentityServices.");
            if (string.IsNullOrWhiteSpace(jwtAudience))
                throw new InvalidOperationException("Jwt:Audience is required for AddIdentityServices.");

            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            services.AddDbContext<AppIdentityDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
                options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
            });

            var requireConfirmedEmail = configuration.GetValue("Identity:RequireConfirmedEmail", defaultValue: false);

            // Controls whether OnTokenValidated below does a per-request SecurityStamp
            // check (near-instant token revocation) or skips it (cheaper, but tokens
            // stay valid until natural expiry even after password change / logout-everywhere).
            var enableSecurityStampValidation = configuration.GetValue("Identity:EnableSecurityStampValidation", defaultValue: true);
            var passwordResetTokenExpiry = configuration.GetValue("Identity:PasswordResetTokenExpiryHours", defaultValue: 24); // ← new

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                // Password policy — OWASP-aligned minimums
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredUniqueChars = 4;

                // Lockout — brute-force protection
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;   // lockout applies even before email confirmation

                // User
                options.User.RequireUniqueEmail = true;

                // Sign-in — require confirmed email/phone before allowing sign-in.
                // NOTE: this is set to true here, but actually enforcing it requires
                // the email-confirmation SEND flow to exist first (depends on
                // IEmailService being finished) — see checklist below.
                options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
                options.SignIn.RequireConfirmedAccount = requireConfirmedEmail;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ITokenService, TokenService>();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(passwordResetTokenExpiry);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Don't let the JWT handler silently rename claim types on the way in.
                // By default, .NET remaps "sub" -> ClaimTypes.NameIdentifier and a few
                // others to legacy XML-schema URIs. That's surprising if your code
                // (or anyone reading claims later) expects the raw "sub", "email" etc.
                // as written by TokenService. Keep the claim names exactly as issued.
                options.MapInboundClaims = false;

                // Only send/require tokens over HTTPS — except in local development,
                // where you're often running plain HTTP. Never disable this in
                // anything resembling production; tokens sent over HTTP can be
                // intercepted in transit.
                options.RequireHttpsMetadata = !environment.IsDevelopment();

                // Standard JWT validation: checks signature, issuer, audience, and
                // expiry against the values configured for this app. This alone
                // proves the token is well-formed and unexpired — it does NOT know
                // whether the underlying user has since been disabled, had their
                // password changed, or been logged out. That's what OnTokenValidated
                // below adds.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),

                    // Default is 5 minutes — meaning a token can still be accepted up to
                    // 5 minutes after it technically expired, to tolerate clock drift
                    // between servers. With short-lived tokens (e.g. 15 min) that's a
                    // meaningful chunk of the token's total life. Tightening this to
                    // 1 minute makes "expires in 15 minutes" mean something closer to
                    // what it says.
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    // Runs AFTER the token passes standard validation above, on every
                    // authenticated request. Purpose: give short-lived JWTs a way to be
                    // invalidated mid-lifetime without a token blacklist/revocation
                    // store — by piggybacking on ASP.NET Identity's SecurityStamp,
                    // which Identity already auto-rotates on password change, and which
                    // we can rotate manually for "log out everywhere" / admin-disables-user.
                    //
                    // Flow:
                    //   1. Pull userId + the security_stamp claim baked into the token
                    //      at login time (see TokenService.GenerateTokenAsync).
                    //   2. Look up the user's CURRENT stamp from the database.
                    //   3. If they don't match, the token was issued before some
                    //      security-relevant change (password reset, manual stamp
                    //      rotation, etc.) — reject it even though it's not expired.
                    OnTokenValidated = async context =>
                    {
                        if (!enableSecurityStampValidation)
                            return;

                        var userIdClaim = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                        var stampClaim = context.Principal?.FindFirstValue("security_stamp");

                        // Missing/malformed claims -> reject outright. A token without
                        // these claims wasn't issued by TokenService.GenerateTokenAsync.
                        if (userIdClaim is null || stampClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                        {
                            context.Fail("Invalid token claims.");
                            return;
                        }

                        // TODO: PERFORMANCE — this DB round-trip runs on every authenticated
                        // request. Before going to production, add a cache layer here:
                        //
                        //   Option A (single instance / dev): IMemoryCache
                        //     - key: $"security_stamp:{userId}", value: stamp, TTL: 30-60s
                        //     - cheapest to add, but cache is per-instance — doesn't work
                        //       correctly once you scale to multiple API instances/pods.
                        //
                        //   Option B (multi-instance / prod): IDistributedCache (Redis)
                        //     - same key/TTL pattern, shared across instances
                        //     - requires wiring up a Redis connection string in config
                        //
                        // Either way: invalidate/overwrite the cache entry inside
                        // UpdateSecurityStampAsync calls (password change, logout-everywhere,
                        // admin-disable) so revocation stays near-instant instead of waiting
                        // out the TTL. A 30-60s TTL is usually acceptable even without manual
                        // invalidation, if you'd rather skip that wiring.
                        var identityService = context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();
                        var currentStamp = await identityService.GetSecurityStampAsync(userId, context.HttpContext.RequestAborted);

                        // Empty stamp -> user no longer exists (e.g. soft-deleted, filtered
                        // out by the query filter) or lookup failed. Mismatch -> stamp was
                        // rotated since this token was issued. Either way, reject.
                        if (string.IsNullOrEmpty(currentStamp) || currentStamp != stampClaim)
                            context.Fail("Token is no longer valid.");
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
