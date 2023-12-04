using System;
using System.IO;
using System.Threading.Tasks;
using AudioNoteTranscription.Common;
using AudioNoteTranscription.Whisper;

namespace AudioNoteTranscription.Model
{
    public class TranscriptionModel: ModelBase
    {
        private readonly string DESTINATION_FOLDER = "Transcriptions";

        public event EventHandler MessageRecognized;

        protected virtual void OnMessageRecognized(MessageRecognizedEventArgs e)
        {
            MessageRecognized?.Invoke(this, e);
        }
        public TranscriptionModel() { } 

        //Add await once is all hooked.
        public async Task<string> TranscribeAsync(string audioFilePath, string language, string modelPath, bool isRealtime)
        {
            // check file was selected.
            if (string.IsNullOrEmpty(audioFilePath) && !isRealtime)
            {
                return String.Empty;
            }

            var result = await Task.Run(string () =>
            {
                var config = new WhisperConfig(modelPath, audioFilePath, language);

                var inference = new Inference();

                inference.MessageRecognized += Inference_MessageRecognized;
                if (isRealtime)
                {
                    var whisperResult = inference.RunRealtime(config);
                    return whisperResult;
                    
                }
                else
                {
                    var whisperResult = inference.RunFromFile(config);
                    return whisperResult;
                }
               

                inference.MessageRecognized -= Inference_MessageRecognized;

               

            });
            return result;
        }

        private void Inference_MessageRecognized(object? sender, EventArgs e)
        {
            OnMessageRecognized(e as MessageRecognizedEventArgs);
        }

        // Simple file storage for the transcribed note, this can be elaborated with different formats etc.
        public async Task StoreTranascription(string noteName, string content)
        {
            await Task.Run(() =>
            {
                // Create the folder if it does not exist
                if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), DESTINATION_FOLDER)))
                {
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), DESTINATION_FOLDER));
                }

                // Combine the name and folder to save
                string path = Path.Combine(Directory.GetCurrentDirectory(), DESTINATION_FOLDER, noteName);

                File.WriteAllText(path, content);
            });
        }
    }
}
