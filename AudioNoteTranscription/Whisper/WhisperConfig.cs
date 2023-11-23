using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AudioNoteTranscription.Whisper
{
    public class WhisperConfig
    {
        public enum ExecutionProvider
        {
            DirectML = 0,
            Cuda = 1,
            Cpu = 2,
        }
        // default props
        public ExecutionProvider ExecutionProviderTarget = ExecutionProvider.Cpu;
        public int DeviceId = 0;

        public int min_length = 0;
        // Max length per inference
        public int max_length = 448;
        public float repetition_penalty = 1.0f;
        public int no_repeat_ngram_size = 1;
        public int num_beams = 1;
        public int num_return_sequences = 1;
        public float length_penalty = 0.2f;
        public Tensor<int> attention_mask;
        public DenseTensor<float> audio;

        public int nFrames = 3000;
        public int hopLength = 160;
        public int sampleRate = 16000;
        public int nMels = 80;

        public string Language = "en";
        public string WhisperOnnxPath = "";
        public string TestAudioPath = "";


        public void SetModelPaths()
        {
            // For some editors the dynamic path doesnt work. If you need to change to a static path
            // update the useStaticPath to true and update the paths below.

            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "Onnx");
            WhisperOnnxPath = Directory.EnumerateFiles(modelPath, "*.onnx", SearchOption.AllDirectories).First();
        }


        public SessionOptions GetSessionOptionsForEp()
        {
            var sessionOptions = new SessionOptions();

            switch (ExecutionProviderTarget)
            {
                case ExecutionProvider.DirectML:
                    sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                    sessionOptions.EnableMemoryPattern = false;
                    sessionOptions.AppendExecutionProvider_DML(DeviceId);
                    sessionOptions.AppendExecutionProvider_CPU();
                    break;
                case ExecutionProvider.Cpu:
                    sessionOptions.AppendExecutionProvider_CPU();
                    break;
                case ExecutionProvider.Cuda:
                    sessionOptions.AppendExecutionProvider_CUDA();
                    break;
                default:
                    sessionOptions.AppendExecutionProvider_CPU();
                    break;
            }

            sessionOptions.RegisterOrtExtensions();
            return sessionOptions;

        }
    }
}

