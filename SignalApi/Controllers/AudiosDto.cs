namespace SignalApi.Controllers
{
    public class AudiosDto
    {
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier for the device that recorded the audio.
        /// If Android device: "Android-UniqueAndroidDeviceId"
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Request was initiated by the server.
        /// </summary>
        public bool RequestedByServer { get; set; }

        public AudioRecordingDto[] Recordings { get; set; } = [];
    }
}