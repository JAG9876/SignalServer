using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SignalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SignalsController : ControllerBase
    {
        [HttpPost("wav")]
        public IActionResult AddWavs(IEnumerable<Signal> signals)
        {
            return Ok(new { Message = "Wavs received", Count = signals.Count() });
        }
    }
}
