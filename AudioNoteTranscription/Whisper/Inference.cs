using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NReco.VideoConverter;

namespace AudioNoteTranscription.Whisper
{
    public static class Inference
    {
        public static Dictionary<int, string> ALL_LANGUAGE_TOKENS => new Dictionary<int, string>
        {
            { 50259, "en" },
            { 50260, "zh" },
            { 50261, "de" },
            { 50262, "es" },
            { 50263, "ru" },
            { 50264, "ko" },
            { 50265, "fr" },
            { 50266, "ja" },
            { 50267, "pt" },
            { 50268, "tr" },
            { 50269, "pl" },
            { 50270, "ca" },
            { 50271, "nl" },
            { 50272, "ar" },
            { 50273, "sv" },
            { 50274, "it" },
            { 50275, "id" },
            { 50276, "hi" },
            { 50277, "fi" },
            { 50278, "vi" },
            { 50279, "he" },
            { 50280, "uk" },
            { 50281, "el" },
            { 50282, "ms" },
            { 50283, "cs" },
            { 50284, "ro" },
            { 50285, "da" },
            { 50286, "hu" },
            { 50287, "ta" },
            { 50288, "no" },
            { 50289, "th" },
            { 50290, "ur" },
            { 50291, "hr" },
            { 50292, "bg" },
            { 50293, "lt" },
            { 50294, "la" },
            { 50295, "mi" },
            { 50296, "ml" },
            { 50297, "cy" },
            { 50298, "sk" },
            { 50299, "te" },
            { 50300, "fa" },
            { 50301, "lv" },
            { 50302, "bn" },
            { 50303, "sr" },
            { 50304, "az" },
            { 50305, "sl" },
            { 50306, "kn" },
            { 50307, "et" },
            { 50308, "mk" },
            { 50309, "br" },
            { 50310, "eu" },
            { 50311, "is" },
            { 50312, "hy" },
            { 50313, "ne" },
            { 50314, "mn" },
            { 50315, "bs" },
            { 50316, "kk" },
            { 50317, "sq" },
            { 50318, "sw" },
            { 50319, "gl" },
            { 50320, "mr" },
            { 50321, "pa" },
            { 50322, "si" },
            { 50323, "km" },
            { 50324, "sn" },
            { 50325, "yo" },
            { 50326, "so" },
            { 50327, "af" },
            { 50328, "oc" },
            { 50329, "ka" },
            { 50330, "be" },
            { 50331, "tg" },
            { 50332, "sd" },
            { 50333, "gu" },
            { 50334, "am" },
            { 50335, "yi" },
            { 50336, "lo" },
            { 50337, "uz" },
            { 50338, "fo" },
            { 50339, "ht" },
            { 50340, "ps" },
            { 50341, "tk" },
            { 50342, "nn" },
            { 50343, "mt" },
            { 50344, "sa" },
            { 50345, "lb" },
            { 50346, "my" },
            { 50347, "bo" },
            { 50348, "tl" },
            { 50349, "mg" },
            { 50350, "as" },
            { 50351, "tt" },
            { 50352, "haw" },
            { 50353, "ln" },
            { 50354, "ha" },
            { 50355, "ba" },
            { 50356, "jw" },
            { 50357, "su" }
        };
        public static List<NamedOnnxValue> CreateOnnxWhisperModelInput(WhisperConfig config)
        {
            // Create a new DenseTensor with the desired shape

            var input = new List<NamedOnnxValue> {
                 NamedOnnxValue.CreateFromTensor("audio_stream", config.audio),
                NamedOnnxValue.CreateFromTensor("min_length", new DenseTensor<int>(new int[] {config.min_length}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("max_length", new DenseTensor<int>(new int[] {config.max_length}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("num_beams", new DenseTensor<int>(new int[] {config.num_beams}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("num_return_sequences", new DenseTensor<int>(new int[] {config.num_return_sequences}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("length_penalty", new DenseTensor<float>(new float[] {config.length_penalty}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("repetition_penalty", new DenseTensor<float>(new float[] {config.repetition_penalty}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("decoder_input_ids", new DenseTensor<int>(new int[]{  50258 , 50263 /*ru*/, 50359, 50363 }, new int[] { 1, 4 } ))
                };

            return input;

        }
        public static string Run(WhisperConfig config, bool useCloudInference)
        {
            // load audio and pad/trim it to fit 30 seconds
            // float[] pcmAudioData = LoadAndProcessAudioFile(config.TestAudioPath, config.sampleRate);
            byte[] audoDataRaw = LoadAudioFileRaw(config.TestAudioPath);
            // Create audio data tensor of shape [1,480000]
            // config.audio = new DenseTensor<float>(pcmAudioData, new[] { 1, pcmAudioData.Length });
            config.audio = new DenseTensor<byte>(audoDataRaw, new[] { 1, audoDataRaw.Length });
            // Create tensor of zeros with shape [1,80,3000]
            config.attention_mask = new DenseTensor<int>(new[] { 1, config.nMels, config.nFrames });

            var input = CreateOnnxWhisperModelInput(config);


            // Check for internet connection
            // var isConnectivity = CheckForInternetConnection();

            // Run inference
            var run_options = new RunOptions();

            if (useCloudInference)
            {
                config.ExecutionProviderTarget = WhisperConfig.ExecutionProvider.Azure;
                run_options.AddRunConfigEntry("use_azure", "1");
                run_options.AddRunConfigEntry("azure.auth_key", "");
            }
            // Set EP
            var sessionOptions = config.GetSessionOptionsForEp();
            sessionOptions.RegisterOrtExtensions();


            var session = new InferenceSession(config.WhisperOnnxPath, sessionOptions);

            List<string> outputs = new List<string>() { "str" };
            var result = session.Run(input, outputs, run_options);

            var stringOutput = (result.ToList().First().Value as IEnumerable<string>).ToArray();

            return stringOutput[0];
        }
        public static byte[] LoadAudioFileRaw(string file)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(file,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(file).Length;
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        public static float[] LoadAndProcessAudioFile(string file, int sampleRate)
        {
            var ffmpeg = new FFMpegConverter();
            var output = new MemoryStream();

            var extension = System.IO.Path.GetExtension(file).Substring(1);

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
                                }); ;
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
            else
            {
                //TODO: batch audio files that are longer than 30 seconds
                // Cut off anything over 30 seconds
                Array.Resize(ref result, 480000);
            }

            return result;
        }

        public static bool CheckForInternetConnection(int timeoutMs = 3000)
        {
            try
            {
                var url = "https://www.bing.com/";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
