using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace SignalApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> LoginWithGoogle(string idToken)
        {
            try
            {
                var Payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                var result = Ok(new { Message = $"Identity confirmed: {Payload.Email}" });

                return result;
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = "Invalid ID token", Error = ex.Message });
            }
        }
    }
}
