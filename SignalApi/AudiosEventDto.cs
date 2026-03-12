namespace SignalApi
{
    public class AudiosEventDto
    {
        public string CorrelationId { get; internal set; }
        public string DeviceId { get; internal set; }
        public bool RequestedByServer { get; internal set; }
        public AudioRecordingDto[] Recordings { get; internal set; }
    }
}
