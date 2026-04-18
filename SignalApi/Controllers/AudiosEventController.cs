using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Diagnostics;

namespace SignalApi.Controllers
{
    [ApiController]
    [Route("api/v1/audio")]
    [EnableRateLimiting("PerDevicePolicy")]
    public class AudiosEventController : ControllerBase
    {
        private readonly IMessageProducer _messageProducer;
        private readonly ILogger<AudiosEventController> _logger;
        private readonly ApiMetrics _apiMetrics;
        private readonly TokenManager _tokenManager;

        public AudiosEventController(IMessageProducer messageProducer, ILogger<AudiosEventController> logger, 
            TokenManager tokenManager, ApiMetrics apiMetrics)
        {
            _messageProducer = messageProducer;
            _tokenManager = tokenManager;
            _logger = logger;
            _apiMetrics = apiMetrics;
        }

        [HttpPost]
        public async Task<IActionResult> AddAudiosEvent(AudiosEventModel audios)
        {
            var sw = Stopwatch.StartNew();
            _apiMetrics.RecordRequest("POST", "api/v1/audio");

            var bearer = Request.Headers["bearer"].FirstOrDefault();

            if (string.IsNullOrEmpty(bearer) || !_tokenManager.ValidateBearerToken(bearer))
            {
                const string msg = "Invalid or missing bearer token";
                _logger.LogWarning(msg);
                Response.Headers.WWWAuthenticate = "Bearer";
                return Unauthorized(new { Message = msg });
            }

            var deviceId = _tokenManager.ExtractDeviceIdFromToken(bearer);
            if (string.IsNullOrEmpty(deviceId))
            {
                const string msg = "Unable to extract device ID from token";
                _logger.LogWarning(msg);
                Response.Headers.WWWAuthenticate = "Bearer";
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
            _logger.LogInformation("Received audios event from device {DeviceId} with correlation ID {CorrelationId}", deviceId, audios.CorrelationId);
            _logger.LogInformation("Bearer = {Bearer}", bearer);

            sw.Stop();
            _apiMetrics.RecordResponseTime(sw.ElapsedMilliseconds, "success");

            return Ok(new { Message = "Audios received", Count = audios.Recordings.Length });
        }

        [HttpGet]
        [Route("instructions")]
        public IActionResult Instructions()
        {
            var sw = Stopwatch.StartNew();
            _apiMetrics.RecordRequest("GET", "api/v1/audio/instructions");

            // Server returns instructions to the client.
            // Some response examples are:
            //   - send specific audio clips to server (need to specify which clips by buffer index and read time)
            //   - tell the client to pull for new instructions more frequently for a period of 10 minutes

            sw.Stop();
            _apiMetrics.RecordResponseTime(sw.ElapsedMilliseconds, "success");

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
