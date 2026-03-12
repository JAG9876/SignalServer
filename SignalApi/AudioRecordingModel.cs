namespace SignalApi
{
    public class AudioRecordingModel
    {
        /// <summary>
        /// Number of milliseconds since midnight 1970_01_01 (UTC)
        /// </summary>
        public long ReadTime { get; set; }

        /// <summary>
        /// Where in the device ringbuffer this audio was stored
        /// </summary>
        public int BufferIndex { get; set; }

        // Assumes all recordings are in mono channel, 16 bit PCM, at 44100 Hz sample rate.
        public short[] Audio { get; set; } = [];
    }
}
