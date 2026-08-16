using API.Template.Application.Common.Models;
using API.Template.Application.Interfaces;
using API.Template.Application.Modules.Users.Dtos;
using API.Template.Identity;
using API.Template.Identity.Interfaces;
using API.Template.Infrastructure.Configuration.Options;
using API.Template.WebApi.Models;
using API.Template.WebApi.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Template.WebApi.Controllers.Test
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly DatabaseOptions _databaseOptions;
        private readonly ISettings _settings;
        private readonly IKeys _keys;
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly ICurrentUserService _currentUser;

        public TestController(IOptions<DatabaseOptions> databaseOptions, IKeys keys, ISettings setting, IIdentityService identityService, ITokenService tokenService, ICurrentUserService currentUser)
        {
            _databaseOptions = databaseOptions.Value;
            _settings = setting;
            _keys = keys;
            _identityService = identityService;
            _tokenService = tokenService;
            _currentUser = currentUser;
        }

        [HttpGet("database")]
        [AllowAnonymous]
        public IActionResult GetDatabaseConfiguration()
        {
            return Ok(_databaseOptions);
        }

        /// <summary>
        /// Load and return IKeys and ISettings for testing purposes
        /// Sensitive keys are masked for security
        /// </summary>
        [HttpGet("config-and-keys")]
        [AllowAnonymous]
        public IActionResult LoadKeysAndSettings()
        {
            try
            {
                var response = new
                {
                    settings = new
                    {
                        _settings.Environment,
                        _settings.DbCommandTimeoutSeconds,
                        _settings.BlobContainerName,
                        _settings.SendGridFromEmail,
                        _settings.JwtIssuer,
                        _settings.JwtAudience,
                        _settings.JwtExpiryMinutes,
                        _settings.ExternalApiBaseUrl,
                        featureFlags = new
                        {
                            exampleFlag = _settings.FeatureFlag("example-flag", false)
                        }
                    },
                    keys = new
                    {
                        databaseConnectionString = MaskSensitiveValue(_keys.DatabaseConnectionString),
                        sendGridApiKey = MaskSensitiveValue(_keys.SendGridApiKey),
                        blobStorageConnectionString = MaskSensitiveValue(_keys.BlobStorageConnectionString),
                        jwtSigningKey = MaskSensitiveValue(_keys.JwtSigningKey),
                        externalApiKey = MaskSensitiveValue(_keys.ExternalApiKey)
                    },
                    database = new
                    {
                        _databaseOptions.ConnectionString,
                        _databaseOptions.Provider
                    },
                    loadedAt = DateTime.UtcNow,
                    status = "All configurations loaded successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, details = ex.StackTrace });
            }
        }

        /// <summary>
        /// Helper method to mask sensitive values for security
        /// Shows only first 4 characters
        /// </summary>
        private string MaskSensitiveValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "[EMPTY]";

            if (value.Length <= 4)
                return "[MASKED]";

            return $"{value.Substring(0, 4)}...{value.Substring(value.Length - 4)}";
        }

        [HttpPost()]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
        {
            var result = await _identityService.CreateUserAsync(dto.Email, dto.Password, dto.FirstName, dto.LastName);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { userId = result.UserId });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel request, CancellationToken ct)
        {
            var result = await _identityService.ValidateCredentialsAsync(request.Email, request.Password, ct);

            if (result.Status != SignInStatus.Succeeded || result.User is null)
                return Unauthorized(new { status = result.Status.ToString() });

            var (accessToken, refreshToken) = await _tokenService.GenerateTokenPairAsync(
                result.User.Id, result.User.Email, ct);

            return Ok(new { accessToken, refreshToken });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshModel request, CancellationToken ct)
        {
            var result = await _tokenService.RefreshAsync(request.RefreshToken, ct);

            if (!result.Succeeded)
                return Unauthorized(new { error = result.Error });

            return Ok(new { accessToken = result.AccessToken, refreshToken = result.RefreshToken });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserByEmailAsync(string email)
        {
            var user = await _identityService.GetUserByEmailAsync(email);
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var loggedInUser = _currentUser.UserId;
            Console.WriteLine(loggedInUser);
            var user = await _identityService.GetUserByIdAsync(userId);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Logout([FromBody] RefreshModel request, CancellationToken ct)
        {
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ct);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel request, CancellationToken ct)
        {
            var (userId, token) = await _identityService.GeneratePasswordResetTokenAsync(request.Email, ct);

            // TODO: when IEmailService exists, send the reset link/token via email
            // here instead of exposing it in the response.
            if (userId is not null && token is not null)
            {
                // await _emailService.SendPasswordResetEmailAsync(request.Email, token, ct);
            }

            return Ok(new
            {
                message = "If that email is registered, a password reset link has been sent.",
                // TEMPORARY — remove once IEmailService actually sends this instead
                // of returning it directly. Exposing the token in the response
                // defeats the security purpose of email-based reset.
                debugToken = token
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel request, CancellationToken ct)
        {
            var result = await _identityService.ResetPasswordAsync(
                request.Email, request.Token, request.NewPassword, ct);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { message = "Password has been reset successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            if (userId is null || userId == Guid.Empty)
                return Unauthorized();

            var result = await _identityService.ChangePasswordAsync(
                userId.Value, request.CurrentPassword, request.NewPassword, ct);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { message = "Password changed successfully." });
        }

    }
}
