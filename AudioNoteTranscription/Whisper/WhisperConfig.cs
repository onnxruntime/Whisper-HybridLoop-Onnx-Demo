using System.IO;
using System.Linq;
using AudioNoteTranscription.Extensions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AudioNoteTranscription.Whisper
{
    public partial class WhisperConfig
    {

        private readonly string _modelPath;
        private ExecutionProvider ExecutionProviderTarget { get; set; }

        public ModelConfig.Config ModelConfig { get; private set; }
        public string WhisperOnnxPath { get; private set; }
        public string AudioPath { get; internal set; }

        public string Language { get; internal set; }

        public int SampleRate { get; private set; } = 16000;

        public DenseTensor<float> Audio { get; set; }
        public int DeviceId { get; private set; } = 0;

        public WhisperConfig(string modelPath, string audioPath, string language, ExecutionProvider executionProviderTarget = ExecutionProvider.Cpu)
        {
            _modelPath = modelPath;
            ModelConfig = SetConfig();
            WhisperOnnxPath = SetModelPaths();
            AudioPath = audioPath;
            Language = language;
            ExecutionProviderTarget = executionProviderTarget;
        }

        private ModelConfig.Config SetConfig()
        {
            var configPath = Directory.EnumerateFiles(_modelPath, "*model.json", SearchOption.TopDirectoryOnly).First();

            var jsonString = File.ReadAllText(configPath);

            var config = System.Text.Json.JsonSerializer.Deserialize<ModelConfig.Config>(jsonString);

            return config ?? new ModelConfig.Config();
        }

        public string SetModelPaths()
        {
            // For some editors the dynamic path doesnt work. If you need to change to a static path
            // update the useStaticPath to true and update the paths below.

            return Directory.EnumerateFiles(_modelPath, "*.onnx", SearchOption.AllDirectories).First();
        }


        public SessionOptions GetSessionOptionsForEp()
        {
            var sessionOptions = new SessionOptions();

            switch (ExecutionProviderTarget)
            {
                //case ExecutionProvider.DirectML:
                //    sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                //    //sessionOptions.EnableMemoryPattern = false;
                //    sessionOptions.AppendExecutionProvider_DML(DeviceId);
                //    //sessionOptions.AppendExecutionProvider_CPU();
                //    break;
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
            sessionOptions.EnableCpuMemArena = true;
            return sessionOptions;

        }
    }
}

