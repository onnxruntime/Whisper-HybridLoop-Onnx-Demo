using System;
using System.Timers;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;


namespace AudioNoteTranscription
{
    public class AudioDataEventArgs : EventArgs
    {
        public float[] AudioData { get; set; }
    }

    public class Capture : IDisposable
    {
        public event EventHandler<AudioDataEventArgs>? DataAvailable;

        private WasapiCapture wasapiLoopbackСapture;

        private BufferedWaveProvider? bufferedLoopbackWaveProvider;
        private BufferedWaveProvider? bufferedMicWaveProvider;

        private bool inProgress = false;
        private WaveInEvent waveSourceMic;
        private bool disposedValue;
        private Timer timer;

        public Capture()
        {
            wasapiLoopbackСapture = InitWasapiLoopbackCapture();
            InitWaveIn(); //init mic if neccessary
        }

        public void Start()
        {
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
            InitMicAudioBuffer(waveSourceMic);
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
            InitLoopbackAudioBuffer(capture);

            capture.DataAvailable += delegate (object? s, WaveInEventArgs a)
            {
                bufferedLoopbackWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
            };

            capture.RecordingStopped += delegate
            {
                capture.Dispose();
            };
        }

        private void InitLoopbackAudioBuffer(WasapiCapture capture)
        {
            bufferedLoopbackWaveProvider = new BufferedWaveProvider(capture.WaveFormat);
            bufferedLoopbackWaveProvider.DiscardOnBufferOverflow = true;
            bufferedLoopbackWaveProvider.ReadFully = false;
            bufferedLoopbackWaveProvider.BufferDuration = TimeSpan.FromSeconds(60);
        }

        private void InitMicAudioBuffer(WaveInEvent capture)
        {
            bufferedMicWaveProvider = new BufferedWaveProvider(capture.WaveFormat);
            bufferedMicWaveProvider.DiscardOnBufferOverflow = true;
            bufferedMicWaveProvider.ReadFully = false;
            bufferedMicWaveProvider.BufferDuration = TimeSpan.FromSeconds(60);
        }

        private void InitWaveInCaptuer(WaveInEvent capture)
        {
            capture.DataAvailable += delegate (object? s, WaveInEventArgs a)
            {
                bufferedMicWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
            };

            capture.RecordingStopped += delegate
            {
                capture.Dispose();
            };
        }

        private void OnAudioData(object? sender, ElapsedEventArgs e)
        {

            if (!inProgress && (bufferedLoopbackWaveProvider.BufferedDuration > TimeSpan.FromSeconds(1) || bufferedMicWaveProvider.BufferedDuration > TimeSpan.FromSeconds(1)))
            {
                inProgress = true;
                var bufferedLoopBackTime = bufferedLoopbackWaveProvider.BufferedDuration;
                var bufferedMicTime = bufferedMicWaveProvider.BufferedDuration;

                var bufferedTime = bufferedLoopBackTime > bufferedMicTime ? bufferedLoopBackTime : bufferedMicTime;

                var mixer = new MixingSampleProvider(new[] { bufferedLoopbackWaveProvider.ToSampleProvider(), bufferedMicWaveProvider.ToSampleProvider() });

                var waveFormat = new WdlResamplingSampleProvider(mixer.ToMono(), 16000);


                var bufferedFrames = TimeSpanToSamples(bufferedTime, waveFormat.WaveFormat);

                var frames = new float[bufferedFrames];

                waveFormat.Read(frames, 0, bufferedFrames);

                inProgress = false;

                DataAvailable?.Invoke(this, new AudioDataEventArgs()
                {
                    AudioData = frames,
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
                    bufferedLoopbackWaveProvider?.ClearBuffer();
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
