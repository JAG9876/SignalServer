using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SignalApi.Controllers
{
    [ApiController]
    [Route("api/v1/audio")]
    [EnableRateLimiting("PerDevicePolicy")]
    public class AudiosEventController : ControllerBase
    {
        private readonly IMessageProducer _messageProducer;
        private readonly ILogger<AudiosEventController> _logger;
        private readonly TokenManager _tokenManager;

        public AudiosEventController(IMessageProducer messageProducer, ILogger<AudiosEventController> logger, TokenManager tokenManager)
        {
            _messageProducer = messageProducer;
            _tokenManager = tokenManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddAudiosEvent(AudiosEventModel audios)
        {
            var bearer = Request.Headers["bearer"].FirstOrDefault();

            if (string.IsNullOrEmpty(bearer) || !_tokenManager.ValidateBearerToken(bearer))
            {
                var msg = "Invalid or missing bearer token";
                _logger.LogWarning(msg);
                Response.Headers["WWW-Authenticate"] = "Bearer";
                return Unauthorized(new { Message = msg });
            }

            var deviceId = _tokenManager.ExtractDeviceIdFromToken(bearer);
            if (string.IsNullOrEmpty(deviceId))
            {
                var msg = "Unable to extract device ID from token";
                _logger.LogWarning(msg);
                Response.Headers["WWW-Authenticate"] = "Bearer";
                return Unauthorized(new { Message = msg });
            }

            var audiosEvent = new AudiosEventDto
            {
                CorrelationId = audios.CorrelationId,
                DeviceId = deviceId,
                RequestedByServer = audios.RequestedByServer,
                Recordings = MapRecordings(audios.Recordings)
            };
            await _messageProducer.PublishAsync(audiosEvent);
            _logger.LogInformation($"Received audios event from device {deviceId} with correlation ID {audios.CorrelationId}");
            _logger.LogInformation($"Bearer = {bearer}");

            return Ok(new { Message = "Audios received", Count = audios.Recordings.Count() });
        }

        [HttpGet]
        [Route("instructions")]
        public IActionResult Instructions()
        {
            // Server returns instructions to the client.
            // Some response examples are:
            //   - send specific audio clips to server (need to specify which clips by buffer index and read time)
            //   - tell the client to pull for new instructions more frequently for a period of 10 minutes
            return Ok(new { Message = "Here are server instructions for the client" });
        }

        private static AudioRecordingDto[] MapRecordings(AudioRecordingModel[] recordings)
        {
            return recordings.Select(r => new AudioRecordingDto
            {
                ReadTime = r.ReadTime,
                BufferIndex = r.BufferIndex,
                Audio = r.Audio
            }).ToArray();
        }
    }
}
