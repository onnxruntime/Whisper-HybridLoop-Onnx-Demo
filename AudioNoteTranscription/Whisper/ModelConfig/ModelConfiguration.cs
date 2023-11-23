namespace AudioNoteTranscription.Whisper.ModelConfig
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ModelConfiguration
    {
        public ModelAttributes model_attributes { get; set; }
        public bool use_ort_extensions { get; set; }
    }


}
