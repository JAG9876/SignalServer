namespace SignalApi.Controllers
{
    public class AudiosEventModel
    {
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Request was initiated by the server.
        /// </summary>
        public bool RequestedByServer { get; set; }

        public AudioRecordingModel[] Recordings { get; set; } = [];
    }
}