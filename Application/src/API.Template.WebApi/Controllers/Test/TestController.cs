using API.Template.Application.Interfaces;
using API.Template.Infrastructure.Configuration.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Template.WebApi.Controllers.Test
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly DatabaseOptions _databaseOptions;
        private readonly ISettings _settings;
        private readonly IKeys _keys;

        public TestController(IOptions<DatabaseOptions> databaseOptions, IKeys keys, ISettings setting)
        {
            _databaseOptions = databaseOptions.Value;
            _settings = setting;
            _keys = keys;
        }

        [HttpGet("database")]
        public IActionResult GetDatabaseConfiguration()
        {
            return Ok(_databaseOptions);
        }

        /// <summary>
        /// Load and return IKeys and ISettings for testing purposes
        /// Sensitive keys are masked for security
        /// </summary>
        [HttpGet("config-and-keys")]
        public IActionResult LoadKeysAndSettings()
        {
            try
            {
                var response = new
                {
                    settings = new
                    {   _settings.Environment,
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

    }
}
