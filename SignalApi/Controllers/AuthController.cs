using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace SignalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("google")]
        public async Task<IActionResult> ConfirmGoogleIdentity(string idToken)
        {
            try
            {
                var Payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                var result = Ok(new { Message = "Identity confirmed" });

                return result;
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = "Invalid ID token", Error = ex.Message });
            }
        }
    }
}
