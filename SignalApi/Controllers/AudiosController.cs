using Microsoft.AspNetCore.Mvc;

namespace SignalApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AudiosController : ControllerBase
    {
        [HttpPost]
        public IActionResult Add(AudiosDto audios)
        {
            return Ok(new { Message = "Audios received", Count = audios.Recordings.Count() });
        }
    }
}
