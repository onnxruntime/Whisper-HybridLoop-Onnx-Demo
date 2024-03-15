using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AudioNoteTranscription.Extensions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NReco.VideoConverter;
using SileroVad;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AudioNoteTranscription.Whisper
{
    public class MessageRecognizedEventArgs : EventArgs
    {
        public string RecognizedText { get; set; }
    }
    public partial class Inference : IDisposable
    {
        private static readonly Vad vad = new();

        public event EventHandler? MessageRecognized;
        public Capture? Capture => capture;
        private bool stop = false;

        public Inference(bool isRealtime)
        {
            if (isRealtime)
            {
                this.capture = new Capture();

                capture.DataAvailable += Capture_DataAvailable;
            }
        }

        protected virtual void OnMessageRecognized(MessageRecognizedEventArgs e)
        {
            MessageRecognized?.Invoke(this, e);
        }
        public static Dictionary<string, int> ALL_LANGUAGE_TOKENS => new Dictionary<string, int>
        {
            { "en", 50259 },
            { "zh", 50260 },
            { "de", 50261 },
            { "es", 50262 },
            { "ru", 50263 },
            { "ko", 50264 },
            { "fr", 50265 },
            { "ja", 50266 },
            { "pt", 50267 },
            { "tr", 50268 },
            { "pl", 50269 },
            { "ca", 50270 },
            { "nl", 50271 },
            { "ar", 50272 },
            { "sv", 50273 },
            { "it", 50274 },
            { "id", 50275 },
            { "hi", 50276 },
            { "fi", 50277 },
            { "vi", 50278 },
            { "he", 50279 },
            { "uk", 50280 },
            { "el", 50281 },
            { "ms", 50282 },
            { "cs", 50283 },
            { "ro", 50284 },
            { "da", 50285 },
            { "hu", 50286 },
            { "ta", 50287 },
            { "no", 50288 },
            { "th", 50289 },
            { "ur", 50290 },
            { "hr", 50291 },
            { "bg", 50292 },
            { "lt", 50293 },
            { "la", 50294 },
            { "mi", 50295 },
            { "ml", 50296 },
            { "cy", 50297 },
            { "sk", 50298 },
            { "te", 50299 },
            { "fa", 50300 },
            { "lv", 50301 },
            { "bn", 50302 },
            { "sr", 50303 },
            { "az", 50304 },
            { "sl", 50305 },
            { "kn", 50306 },
            { "et", 50307 },
            { "mk", 50308 },
            { "br", 50309 },
            { "eu", 50310 },
            { "is", 50311 },
            { "hy", 50312 },
            { "ne", 50313 },
            { "mn", 50314 },
            { "bs", 50315 },
            { "kk", 50316 },
            { "sq", 50317 },
            { "sw", 50318 },
            { "gl", 50319 },
            { "mr", 50320 },
            { "pa", 50321 },
            { "si", 50322 },
            { "km", 50323 },
            { "sn", 50324 },
            { "yo", 50325 },
            { "so", 50326 },
            { "af", 50327 },
            { "oc", 50328 },
            { "ka", 50329 },
            { "be", 50330 },
            { "tg", 50331 },
            { "sd", 50332 },
            { "gu", 50333 },
            { "am", 50334 },
            { "yi", 50335 },
            { "lo", 50336 },
            { "uz", 50337 },
            { "fo", 50338 },
            { "ht", 50339 },
            { "ps", 50340 },
            { "tk", 50341 },
            { "nn", 50342 },
            { "mt", 50343 },
            { "sa", 50344 },
            { "lb", 50345 },
            { "my", 50346 },
            { "bo", 50347 },
            { "tl", 50348 },
            { "mg", 50349 },
            { "as", 50350 },
            { "tt", 50351 },
            { "haw", 50352 },
            { "ln", 50353 },
            { "ha", 50354 },
            { "ba", 50355 },
            { "jw", 50356 },
            { "su", 50357 }
        };

        public bool Stop { get => stop; set => stop = value; }

        public bool ClearText { get; set; } = false;
        public string FullText { get => fullText; set => fullText = value; }

        public bool Translate { get; set; } = false;

        private string fullText = string.Empty;

        public List<NamedOnnxValue> CreateOnnxWhisperModelInput(WhisperConfig config, Memory<float> audio)
        {
            // Create a new DenseTensor with the desired shape

            var language = ALL_LANGUAGE_TOKENS[config.Language]; /*ru*/

            var modelConfig = config.ModelConfig.config.model_attributes;

            var input = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("audio_pcm", new DenseTensor<float>(audio, new[] { 1, audio.Length })),
                NamedOnnxValue.CreateFromTensor("min_length", new DenseTensor<int>(new int[] { modelConfig.min_length }, new int[] { 1 })),

                NamedOnnxValue.CreateFromTensor("num_beams", new DenseTensor<int>(new int[] { modelConfig.num_beams }, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("num_return_sequences", new DenseTensor<int>(new int[] { modelConfig.num_return_sequences }, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("length_penalty", new DenseTensor<float>(new float[] { modelConfig.length_penalty }, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("repetition_penalty", new DenseTensor<float>(new float[] { modelConfig.repetition_penalty }, new int[] { 1 })),
                };


            int[]? inputParameters = null;

            if (string.Equals(modelConfig.NameOrPath, "openai/whisper-large-v3", StringComparison.OrdinalIgnoreCase))
            {
                //50365 - 51865

                inputParameters = [
                    50258,
                    language,
                    //50361,  //startoflm
                    //50362,  //startofprev
                    50363,  //nospeech
                    //50364,  //notimestamps
                    50365,     //<|0.00|>
                ];

                if (Translate)
                {
                    inputParameters = [.. inputParameters, 50359]; //translate
                }
                else
                {
                    inputParameters = [.. inputParameters, 50360]; //transcribe
                }
            }
            else
            {
                inputParameters = [
                    50258,
                    language,
                    50363,     //nospeech
                    50365,     //<|0.00|>
                ];

                if (Translate)
                {
                    inputParameters = [.. inputParameters, 50358]; //translate
                }
                else
                {
                    inputParameters = [.. inputParameters, 50359]; //transcribe
                }
            }

            Array.Sort(inputParameters);

            input.Add(NamedOnnxValue.CreateFromTensor("decoder_input_ids", new DenseTensor<int>(inputParameters, new int[] { 1, inputParameters.Length })));
            input.Add(NamedOnnxValue.CreateFromTensor("max_length", new DenseTensor<int>(new int[] { modelConfig.max_length }, new int[] { 1 })));

            return input;
        }

        public string RunFromFile(WhisperConfig config)
        {
            float[] pcmAudioData = LoadAndProcessAudioFile(config.AudioPath, config.SampleRate);

            // Run inference
            using var runOptions = new RunOptions();
            // Set EP
            using var sessionOptions = config.GetSessionOptionsForEp();

            using var session = new InferenceSession(config.WhisperOnnxPath, sessionOptions);

            return Run(config, pcmAudioData, runOptions, session);
        }

        public float[] MergeUsingBlockCopy(float[] firstArray, float[] secondArray)
        {
            var combinedArray = new float[firstArray.Length + secondArray.Length];
            firstArray.CopyTo(combinedArray, 0);
            secondArray.CopyTo(combinedArray, firstArray.Length);
            return combinedArray;
        }

        public string RunRealtime(WhisperConfig config, bool isMic, bool isLoopback)
        {

            // Run inference
            using var runOptions = new RunOptions();

            // Set EP
            using var sessionOptions = config.GetSessionOptionsForEp();
            using var session = new InferenceSession(config.WhisperOnnxPath, sessionOptions);

            capture?.Start();

            if (isLoopback)
            {
                capture?.StartLoopback();
            }

            if (isMic)
            {
                capture?.StartMic();
            }

            var N_SAMPLES = 480000;
            float[] audioDataArray = new float[N_SAMPLES];
            int position = 0;
            string temporaryRecognized = string.Empty;

            while (!Stop)
            {
                if (ClearText)
                {
                    ClearText = false;

                    fullText = string.Empty;
                    temporaryRecognized = string.Empty;
                    audioDataArray = new float[N_SAMPLES];
                    waitingList.Clear();
                }

                if (!waitingList.IsEmpty)
                {
                    inProgress = true;

                    if (waitingList.TryPeek(out AudioDataEventArgs e))
                    {
                        float[] audioData = null;

                        while (waitingList.TryDequeue(out AudioDataEventArgs audioChank))
                        {
                            if (audioData == null)
                            {
                                audioData = audioChank.AudioData;
                            }
                            else
                            {
                                audioData = MergeUsingBlockCopy(audioData, audioChank.AudioData);
                            }

                        }

                        List<VadSpeech> result = vad.GetSpeechTimestamps(audioData, min_silence_duration_ms: 500, threshold: 0.4f);

                        if (result.Count > 0 && audioData != null)
                        {
                            if (audioData.Length + position > N_SAMPLES)
                            {
                                temporaryRecognized = SaveResultOfRecognition(N_SAMPLES, out audioDataArray, out position, temporaryRecognized, audioData);
                            }
                            else
                            {
                                audioData.CopyTo(audioDataArray, position);
                            }
                            position += audioData.Length;


                            var realTimeRecognizedText = RemoveTimeStamps(RunRealtime(config, audioDataArray, runOptions, session));

                            if (!string.IsNullOrWhiteSpace(realTimeRecognizedText) && temporaryRecognized.Length < realTimeRecognizedText.Length)
                            {
                                temporaryRecognized = SplitInToSentences(realTimeRecognizedText);
                            }
                            else
                            {
                                SaveResultOfRecognition(N_SAMPLES, out audioDataArray, out position, temporaryRecognized, audioData);
                                temporaryRecognized = RemoveTimeStamps(RunRealtime(config, audioDataArray, runOptions, session));
                            }


                            OnMessageRecognized(
                                new MessageRecognizedEventArgs()
                                {
                                    RecognizedText = fullText + temporaryRecognized

                                });
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(temporaryRecognized))
            {
                fullText += temporaryRecognized;

                fullText = SplitInToSentences(fullText);
            }

            return fullText;
        }

        private string SaveResultOfRecognition(int N_SAMPLES, out float[] audioDataArray, out int position, string temporaryRecognized, float[] audioData)
        {
            position = 0;
            audioDataArray = audioData.Length > N_SAMPLES ? new float[audioData.Length] : new float[N_SAMPLES];

            audioData.CopyTo(audioDataArray, position);


            fullText += SplitInToSentences(temporaryRecognized);

            temporaryRecognized = string.Empty;

            OnMessageRecognized(
                new MessageRecognizedEventArgs()
                {
                    RecognizedText = fullText

                });
            return temporaryRecognized;
        }

        private static string SplitInToSentences(string fullText)
        {
            // Create a Regex object using the regular expression.
            var regex = SpliToSentencesRegex();

            fullText = regex.Replace(fullText, delegate (Match match)
            {
                return "\r\n";
            });
            return fullText;
        }

        private static string RemoveTimeStamps(string fullText)
        {
            // Create a Regex object using the regular expression.

            return SpliToTimeRegex().Replace(fullText, string.Empty);
        }

        private static string SplitToTimeStamps(string fullText, float startTime)
        {
            // Create a Regex object using the regular expression.
            var regex = SpliToTimeRegex();

            fullText = regex.Replace(fullText, delegate (Match match)
            {
                var time = float.Parse(match.Groups["time"].Value) + startTime;
                return "\r\n" + TimeSpan.FromSeconds(time) + "\r\n";
            });
            return fullText;
        }

        private bool inProgress = false;
        private ConcurrentQueue<AudioDataEventArgs> waitingList = new();
        private Capture? capture;
        private bool disposedValue;

        private void Capture_DataAvailable(object? sender, AudioDataEventArgs e)
        {
            waitingList.Enqueue(e);
        }

        public string RunRealtime(WhisperConfig config, float[] pcmAudioData, RunOptions runOptions, InferenceSession session)
        {
            StringBuilder stringBuilder = new();
            foreach (var audio in pcmAudioData.ChunkViaMemory(480000))
            {
                var input = CreateOnnxWhisperModelInput(config, audio);
                try
                {
                    var result = session.Run(input, ["str"], runOptions);

                    stringBuilder.Append((result.FirstOrDefault()?.Value as IEnumerable<string>)?.First() ?? string.Empty);
                }
                catch (Exception ex)
                {

                }

            }
            return stringBuilder.ToString();
        }

        public string Run(WhisperConfig config, float[] pcmAudioData, RunOptions runOptions, InferenceSession session)
        {
            float startTime = 0f;
            StringBuilder stringBuilder = new();
            foreach (var audio in pcmAudioData.ChunkViaMemory(480000))
            {
                var input = CreateOnnxWhisperModelInput(config, audio);
                var result = session.Run(input, ["str"], runOptions);

                var recognizedText = SplitToTimeStamps((result.FirstOrDefault()?.Value as IEnumerable<string>)?.First() ?? string.Empty, startTime);

                stringBuilder.Append(recognizedText);

                startTime += 30f;

                OnMessageRecognized(new MessageRecognizedEventArgs() { RecognizedText = stringBuilder.ToString() });

                if (Stop)
                {
                    break;
                }
            }

            return stringBuilder.ToString();
        }

        public static float[] LoadAndProcessAudioFile(string file, int sampleRate)
        {
            var ffmpeg = new FFMpegConverter();
            var output = new MemoryStream();

            var extension = Path.GetExtension(file).Substring(1);

            // Convert to PCM
            ffmpeg.ConvertMedia(inputFile: file,
                                inputFormat: extension,
                                outputStream: output,
                                //  DE s16le PCM signed 16-bit little-endian
                                outputFormat: "s16le",
                                new ConvertSettings()
                                {
                                    AudioCodec = "pcm_s16le",
                                    AudioSampleRate = sampleRate,
                                    // Convert to mono
                                    CustomOutputArgs = "-ac 1"
                                });
            var buffer = output.ToArray();
            //The buffer length is divided by 2 because each sample in
            //the raw PCM format is encoded as a signed 16-bit integer,
            //which takes up 2 bytes of memory. Dividing the buffer
            //length by 2 gives the number of samples in the audio data.
            var result = new float[buffer.Length / 2];
            for (int i = 0; i < buffer.Length; i += 2)
            {
                short sample = (short)(buffer[i + 1] << 8 | buffer[i]);


                //The division by 32768 is used to normalize the audio data
                //to have values between -1.0 and 1.0.
                //The raw PCM format used by ffmpeg encodes audio samples
                //as signed 16-bit integers with a range from -32768
                //to 32767. Dividing by 32768 scales the samples to have
                //a range from -1.0 to 1.0 in floating-point format.
                result[i / 2] = sample / 32768.0f;
            }

            // Add padding to the audio file to make it 30 seconds long
            int paddingLength = sampleRate * 30 - result.Length;
            if (paddingLength > 0)
            {
                Array.Resize(ref result, result.Length + paddingLength);
            }

            return result;
        }

        // Define the regular expression that you want to use to split the text into sentences.
        [GeneratedRegex(@"(?<=[.!?])\s+(?=[A-Za-z-А-Яа-я])", RegexOptions.Compiled)]
        private static partial Regex SpliToSentencesRegex();

        // Define the regular expression that you want to use to split the text into sentences.
        [GeneratedRegex(@"<\|(?'time'\d+\.\d+)\|>", RegexOptions.Compiled)]
        private static partial Regex SpliToTimeRegex();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    capture?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Inference()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
