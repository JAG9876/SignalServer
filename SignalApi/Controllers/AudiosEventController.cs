using Microsoft.AspNetCore.Mvc;

namespace SignalApi.Controllers
{
    [ApiController]
    [Route("api/v1/audio")]
    public class AudiosEventController : ControllerBase
    {
        private readonly IMessageProducer _messageProducer;
        private readonly ILogger<AudiosEventController> _logger;

        public AudiosEventController(IMessageProducer messageProducer, ILogger<AudiosEventController> logger)
        {
            _messageProducer = messageProducer;
            _logger = logger;
        }

        [HttpPost]
       public async Task<IActionResult> AddAudiosEvent(AudiosEventModel audios)
        {
            var audiosEvent = new AudiosEventDto
            {
                CorrelationId = audios.CorrelationId,
                DeviceId = audios.DeviceId,
                RequestedByServer = audios.RequestedByServer,
                Recordings = MapRecordings(audios.Recordings)
            };
            await _messageProducer.PublishAsync(audiosEvent);
            _logger.LogInformation($"Received audios event from device {audios.DeviceId} with correlation ID {audios.CorrelationId}");

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
