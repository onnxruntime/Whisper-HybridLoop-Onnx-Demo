namespace AudioNoteTranscription.Whisper.Services
{
    public class HostedModel
    {
        public required string ModelUrl { get; set; }
        public required string ModelConfigUrl { get; set; }

        public required string DownloadSize { get; set; }

    }
}
