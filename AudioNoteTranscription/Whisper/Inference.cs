﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NReco.VideoConverter;

namespace AudioNoteTranscription.Whisper
{
    public static class Inference
    {
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
        public static List<NamedOnnxValue> CreateOnnxWhisperModelInput(WhisperConfig config)
        {
            // Create a new DenseTensor with the desired shape

            var language = ALL_LANGUAGE_TOKENS[config.Language]; /*ru*/

            var input = new List<NamedOnnxValue> {
                 NamedOnnxValue.CreateFromTensor("audio_pcm", config.audio),
                NamedOnnxValue.CreateFromTensor("min_length", new DenseTensor<int>(new int[] {config.min_length}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("max_length", new DenseTensor<int>(new int[] {config.max_length}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("num_beams", new DenseTensor<int>(new int[] {config.num_beams}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("num_return_sequences", new DenseTensor<int>(new int[] {config.num_return_sequences}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("length_penalty", new DenseTensor<float>(new float[] {config.length_penalty}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("repetition_penalty", new DenseTensor<float>(new float[] {config.repetition_penalty}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("decoder_input_ids", new DenseTensor<int>(new int[]{  50258 , language, 50359, 50363 }, new int[] { 1, 4 } ))
                };

            return input;
        }

        public static string Run(WhisperConfig config)
        {
            // load audio and pad/trim it to fit 30 seconds
            float[] pcmAudioData = LoadAndProcessAudioFile(config.TestAudioPath, config.sampleRate);

            // Run inference
            var run_options = new RunOptions();
            // Set EP
            var sessionOptions = config.GetSessionOptionsForEp();
            sessionOptions.RegisterOrtExtensions();

            StringBuilder stringBuilder = new StringBuilder();

            using (var session = new InferenceSession(config.WhisperOnnxPath, sessionOptions))
            {
                foreach (var audio in pcmAudioData.Chunk(480000))
                {
                    config.audio = new DenseTensor<float>(audio, new[] { 1, audio.Length });
                    var input = CreateOnnxWhisperModelInput(config);
                    var result = session.Run(input, ["str"], run_options);

                    stringBuilder.Append((result.FirstOrDefault()?.Value as IEnumerable<string>)?.First() ?? string.Empty);
                }
            }

            sessionOptions.Dispose();
            run_options.Dispose();

            return stringBuilder.ToString();
        }
        public static byte[] LoadAudioFileRaw(string file)
        {
            byte[] buff = null;
            using FileStream fs = new FileStream(file,
                                             FileMode.Open,
                                             FileAccess.Read);
            using BinaryReader br = new BinaryReader(fs);
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
            //else
            //{
            //    //TODO: batch audio files that are longer than 30 seconds
            //    // Cut off anything over 30 seconds
            //    Array.Resize(ref result, 480000);
            //}

            return result;
        }



        //public static IEnumerable<byte> LoadAndProcessAudioFile(string file, int sampleRate)
        //{
        //    var ffmpeg = new FFMpegConverter();
        //    var output = new MemoryStream();

        //    var extension = System.IO.Path.GetExtension(file).Substring(1);

        //    // Convert to PCM
        //    ffmpeg.ConvertMedia(inputFile: file,
        //                        inputFormat: extension,
        //                        outputStream: output,
        //                        //  DE s16le PCM signed 16-bit little-endian
        //                        outputFormat: "s16le",
        //                        new ConvertSettings()
        //                        {
        //                            AudioCodec = "pcm_s16le",
        //                            AudioSampleRate = sampleRate,
        //                            // Convert to mono
        //                            CustomOutputArgs = "-ac 1"
        //                        });
        //    var resalt = output.ToArray();
        //    var waveFormat = new WaveFormat(sampleRate, 16, 1);
        //    IWaveProvider provider = new RawSourceWaveStream(output, waveFormat);

        //    var sampleProvider = provider.ToSampleProvider();


        //   var first = sampleProvider.Take(TimeSpan.FromSeconds(30)).ToWaveProvider();


        //    var writer =  new WaveFileWriter(outputWav, waveFormat);

        //    writer.Write(first);
        //    writer.Flush();


        //    return outputWav.ToArray();


        //}

    }

    class CustomWaveFormat : WaveFormat
    {
        public CustomWaveFormat(int rate, int bits, int channels)
            : base(rate, bits, channels)
        {
            extraSize = 0;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write((int)16); // wave format length
            writer.Write((short)Encoding);
            writer.Write((short)Channels);
            writer.Write((int)SampleRate);
            writer.Write((int)AverageBytesPerSecond);
            writer.Write((short)BlockAlign);
            writer.Write((short)BitsPerSample);
        }
    }
}
