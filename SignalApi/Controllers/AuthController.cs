using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace SignalApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenManager _tokenManager;

        public AuthController(TokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        [HttpPost]
        public async Task<IActionResult> LoginWithGoogle(AuthModel model)
        {
            try
            {
                var Payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken);

                var userDto = GetOrCreateUser(Payload);
                var tokensDto = CreateNewTokens(userDto, model.DeviceId);

                return Ok(new { Message = $"Identity confirmed: {Payload.Email} (id={Payload.Subject})",
                    tokensDto.AccessToken, tokensDto.RefreshToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = "Invalid ID token", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshAccessToken(RefreshAccessTokenModel model)
        {
            try
            {
                /*
                var principal = _tokenManager.ValidateRefreshToken(model.RefreshToken);
                var userId = principal.FindFirst("userId")?.Value;
                var deviceId = principal.FindFirst("deviceId")?.Value;
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(deviceId))
                {
                    return Unauthorized(new { Message = "Invalid refresh token" });
                }
                */
                 var deviceId = _tokenManager.ExtractDeviceIdFromToken(model.RefreshToken);
                 if (string.IsNullOrEmpty(deviceId))
                 {
                     return Unauthorized(new { Message = "Invalid refresh token" });
                }
                return Ok(new { Message = "Access token refreshed", AccessToken = GenerateAccessTokenForDevice(deviceId) });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = "Invalid refresh token", Error = ex.Message });
            }
        }

        private string GenerateAccessTokenForDevice(string deviceId)
        {
            return _tokenManager.GenerateAccessToken(
                "auth.crawlsoft.com",
                "test.crawlsoft.com/api/audio", deviceId);
        }

        private string GenerateRefreshTokenForDevice(string userId, string deviceId)
        {
            return _tokenManager.GenerateRefreshToken(userId, deviceId);
        }

        /// <summary>
        /// Create new accessToken and refreshToken for the device.
        /// If there are already tokens for the device, invalidate them and create new ones.
        /// </summary>
        /// <param name="userDto"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private TokensDto CreateNewTokens(UserDto userDto, string deviceId)
        {
            return new TokensDto
            {
                AccessToken = GenerateAccessTokenForDevice(deviceId),
                RefreshToken = GenerateRefreshTokenForDevice(userDto.Id, deviceId)
            };
        }

        /// <summary>
        /// Identifies or creates user based on Payload.Subject
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private UserDto GetOrCreateUser(GoogleJsonWebSignature.Payload payload)
        {
            return new UserDto { Id = payload.Subject };
        }
    }

    internal class TokensDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    internal class UserDto
    {
        public string Id { get; set; } = string.Empty;
    }
}
