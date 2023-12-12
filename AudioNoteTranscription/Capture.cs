﻿using System;
using System.Timers;
using AudioNoteTranscription.Whisper;
using Microsoft.ML.OnnxRuntime;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;


namespace AudioNoteTranscription
{
    public class AudioDataEventArgs : EventArgs
    {
        public float[] AudioData { get; set; }
        public RunOptions runOptions { get; set; }
        public SessionOptions sessionOptions { get; set; }
        public InferenceSession session { get; set; }

        public WhisperConfig config { get; set; }
    }

    public class Capture : IDisposable
    {
        public event EventHandler<AudioDataEventArgs>? DataAvailable;

        private WasapiCapture wasapiLoopbackСapture;

        private BufferedWaveProvider? bufferedWaveProvider;
        private WhisperConfig config;
        private RunOptions runOptions;
        private SessionOptions sessionOptions;
        private InferenceSession session;

        private bool inProgress = false;
        private WaveInEvent waveSourceMic;
        private bool disposedValue;
        private Timer timer;

        public Capture(WhisperConfig config)
        {
            this.config = config;
            wasapiLoopbackСapture = InitWasapiLoopbackCapture();
            //InitWaveIn(); //init mic if neccessary
        }

        public void Start()
        {


            // Run inference
            this.runOptions = new RunOptions();
            // Set EP
            this.sessionOptions = config.GetSessionOptionsForEp();
            this.session = new InferenceSession(config.WhisperOnnxPath, sessionOptions);


            wasapiLoopbackСapture.StartRecording();
            waveSourceMic?.StartRecording();

            // устанавливаем метод обратного вызова
            // создаем таймер
            this.timer = new Timer(TimeSpan.FromSeconds(1));
            timer.Elapsed += OnAudioData;
            timer.Start();
        }


        private WaveInEvent InitWaveIn()
        {
            this.waveSourceMic = new WaveInEvent();
            waveSourceMic.WaveFormat = wasapiLoopbackСapture.WaveFormat;
            InitWaveInCaptuer(waveSourceMic);

            return this.waveSourceMic;
        }

        private WasapiCapture InitWasapiCapture()
        {

            wasapiLoopbackСapture = new WasapiCapture();
            InitCaptuer(wasapiLoopbackСapture);

            return wasapiLoopbackСapture;
        }

        private WasapiCapture InitWasapiLoopbackCapture()
        {
            wasapiLoopbackСapture = new WasapiLoopbackCapture();

            InitCaptuer(wasapiLoopbackСapture);

            return wasapiLoopbackСapture;
        }

        private void InitCaptuer(WasapiCapture capture)
        {
            InitAudioBuffer(capture);

            capture.DataAvailable += delegate (object? s, WaveInEventArgs a)
            {
                bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
            };

            capture.RecordingStopped += delegate
            {
                capture.Dispose();
            };
        }

        private void InitAudioBuffer(WasapiCapture capture)
        {
            bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat);
            bufferedWaveProvider.DiscardOnBufferOverflow = true;
            bufferedWaveProvider.ReadFully = false;
            bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(60);
        }

        private void InitWaveInCaptuer(WaveInEvent capture)
        {
            capture.DataAvailable += delegate (object? s, WaveInEventArgs a)
            {
                bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
            };

            capture.RecordingStopped += delegate
            {
                capture.Dispose();
            };
        }




        private void OnAudioData(object? sender, ElapsedEventArgs e)
        {

            if (!inProgress && bufferedWaveProvider.BufferedDuration > TimeSpan.FromSeconds(1))
            {
                inProgress = true;
                var bufferedTime = bufferedWaveProvider.BufferedDuration;

                var waveFormat = new WdlResamplingSampleProvider(bufferedWaveProvider.ToSampleProvider(), 16000).ToMono();

                var bufferedFrames = TimeSpanToSamples(bufferedTime, waveFormat.WaveFormat);

                var frames = new float[bufferedFrames];

                waveFormat.Read(frames, 0, bufferedFrames);

                inProgress = false;

                DataAvailable?.Invoke(this, new AudioDataEventArgs()
                {
                    AudioData = frames,
                    runOptions = runOptions,
                    session = session,
                    sessionOptions = sessionOptions,
                    config = config
                });
            }

        }

        private static int TimeSpanToSamples(TimeSpan time, WaveFormat waveFormat)
        {
            return (int)(time.TotalSeconds * (double)waveFormat.SampleRate) * waveFormat.Channels;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Stop();
                    timer.Dispose();
                    wasapiLoopbackСapture.StopRecording();
                    wasapiLoopbackСapture?.Dispose();
                    bufferedWaveProvider?.ClearBuffer();
                    
                    session?.Dispose();
                    sessionOptions?.Close();
                    sessionOptions?.Dispose();
                    runOptions?.Close();
                    runOptions?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
