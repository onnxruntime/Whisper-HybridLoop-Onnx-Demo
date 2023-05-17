using AudioNoteTranscription.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioNoteTranscription.Whisper;
using static System.Net.Mime.MediaTypeNames;

namespace AudioNoteTranscription.Model
{
    public class TranscriptionModel: ModelBase
    {
        private readonly string DESTINATION_FOLDER = "Transcriptions";
        public TranscriptionModel() { } 

        //Add await once is all hooked.
        public async Task<string> TranscribeAsync(string audioFilePath, bool useCloudInference)
        {
            // check file was selected.
            if (string.IsNullOrEmpty(audioFilePath))
            {
                return String.Empty;
            }

            var result = await Task.Run(string () =>
            {
                var config = new WhisperConfig();
                config.SetModelPaths();
                config.TestAudioPath = audioFilePath;

                var whisperResult = Whisper.Inference.Run(config, useCloudInference);
                Console.WriteLine(whisperResult);
                return whisperResult;

            });
            return result;
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

            return;
        }
    }
}
