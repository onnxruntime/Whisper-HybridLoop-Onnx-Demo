using System;
using System.IO;
using System.Threading.Tasks;
using AudioNoteTranscription.Common;
using AudioNoteTranscription.Extensions;
using AudioNoteTranscription.Whisper;
using Windows.Networking.NetworkOperators;

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
        public Task TranscribeAsync(WhisperConfig whisperConfig, bool isRealtime, bool isMic, bool isLoopback, bool isTranslate)
        {
            // check file was selected.
            if (!string.IsNullOrEmpty(whisperConfig.AudioPath) || isRealtime)
            {
                return Task.Run(string () =>
                  {
                      var config = whisperConfig;

                      inference = new Inference(isRealtime);
                      var whisperResult = string.Empty;

                      inference.Translate = isTranslate;

                      inference.MessageRecognized += Inference_MessageRecognized;
                      if (isRealtime)
                      {
                          whisperResult = inference.RunRealtime(config, isMic, isLoopback);
                      }
                      else
                      {
                          whisperResult = inference.RunFromFile(config);
                      }

                      inference.MessageRecognized -= Inference_MessageRecognized;

                      return whisperResult;
                  });
            }

            return Task.CompletedTask;
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

        public async Task<bool> StopRecognitionAsync()
        {
            if (inference?.Stop == false)
            {
                this.inference.Stop = true;

                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        public void StopMic()
        {
            inference?.Capture?.StopMic();
        }

        public void StartMic()
        {
            inference?.Capture?.StartMic();
        }

        public void StopLoopback()
        {
            inference?.Capture?.StopLoopback();
        }

        public void StartLoopback()
        {
            inference?.Capture?.StartLoopback();
        }

        public void ClearText()
        {
            if (inference != null)
            {
                inference.ClearText = true;
                inference.FullText = string.Empty;
            }
        }

        public void SetTranslate(bool isTranslate)
        {
            if (inference != null)
            {
                inference.Translate = isTranslate;
            }
        }

    }
}
