using System;
using System.IO;
using System.Threading.Tasks;
using AudioNoteTranscription.Common;
using AudioNoteTranscription.Extensions;
using AudioNoteTranscription.Whisper;

namespace AudioNoteTranscription.Model
{
    public class TranscriptionModel : ModelBase
    {
        private readonly string DESTINATION_FOLDER = "Transcriptions";

        public event EventHandler MessageRecognized;

        private Inference? inference = null;

        protected virtual void OnMessageRecognized(MessageRecognizedEventArgs e)
        {
            MessageRecognized?.Invoke(this, e);
        }
        public TranscriptionModel() { }

        //Add await once is all hooked.
        public async Task<string> TranscribeAsync(string audioFilePath, string language, string modelPath, bool isRealtime, ExecutionProvider executionProviderTarget)
        {
            // check file was selected.
            if (string.IsNullOrEmpty(audioFilePath) && !isRealtime)
            {
                return String.Empty;
            }

            var result = await Task.Run(string () =>
            {
                var config = new WhisperConfig(modelPath, audioFilePath, language, executionProviderTarget);

                inference = new Inference();
                var whisperResult = string.Empty;

                inference.MessageRecognized += Inference_MessageRecognized;
                if (isRealtime)
                {
                    whisperResult = inference.RunRealtime(config);
                }
                else
                {
                    whisperResult = inference.RunFromFile(config);
                }


                inference.MessageRecognized -= Inference_MessageRecognized;
                return whisperResult;


            });
            return result;
        }

        private void Inference_MessageRecognized(object? sender, EventArgs e)
        {
            OnMessageRecognized(e as MessageRecognizedEventArgs);
        }

        // Simple file storage for the transcribed note, this can be elaborated with different formats etc.
        public async Task StoreTranascriptionAsync(string noteName, string content)
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

        public async Task<bool> StopReognitionAsync()
        {
            if (inference?.Stop == false)
            {
                this.inference.Stop = true;
                
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }
    }
}
