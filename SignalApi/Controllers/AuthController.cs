using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace SignalApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> LoginWithGoogle(AuthModel model)
        {
            try
            {
                var Payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken);

                var userDto = GetOrCreateUser(Payload);
                var tokensDto = CreateNewTokens(userDto, model.DeviceId);

                return Ok(new { Message = $"Identity confirmed: {Payload.Email} (id={Payload.Subject})", tokensDto.AccessToken, tokensDto.RefreshToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = "Invalid ID token", Error = ex.Message });
            }
        }

        /// <summary>
        /// Create new accessToken and refreshToken for the device.
        /// If there are already tokens for the device, invalidate them and create new ones.
        /// </summary>
        /// <param name="userDto"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private static TokensDto CreateNewTokens(UserDto userDto, string deviceId)
        {
            var accessToken = TokenGenerator.GenerateToken(
                "12345678901234561234567890123456", "auth.crawlsoft.com",
                "test.crawlsoft.com/api/audio", deviceId);
            return new TokensDto
            {
                AccessToken = accessToken,
                //AccessToken = $"accesstoken-{userDto.Id}-{deviceId}",
                RefreshToken = $"refreshtoken-{userDto.Id}-{deviceId}"
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
