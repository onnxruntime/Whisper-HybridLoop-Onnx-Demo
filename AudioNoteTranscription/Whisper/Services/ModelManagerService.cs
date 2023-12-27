using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AudioNoteTranscription.Extensions;

namespace AudioNoteTranscription.Whisper.Services
{
    public class ModelManagerService
    {
        public static Dictionary<string, HostedModel> ModelsDictionary = new()
        {
            {"whisper-base-cpu-int4",
                new HostedModel()
                {
                     DownloadSize = "201 MB",
                     ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-base-cpu-int4/resolve/main/whisper_cpu_int4_cpu-cpu_model.json?download=true",
                     ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-base-cpu-int4/resolve/main/whisper_cpu_int4_cpu-cpu_model.onnx?download=true"
                }
            },
            {"whisper-large-v3-int4",
                new HostedModel()
                {
                     DownloadSize = "1.27 GB",
                     ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-large-v3-int4/resolve/main/whisper_gpu_int8_gpu-cuda_model.json?download=true",
                     ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-large-v3-int4/resolve/main/whisper_gpu_int8_gpu-cuda_model.onnx?download=true"
                }
            },
            {"whisper-large-v2-int4",
                new HostedModel()
                {
                     DownloadSize = "1.26 GB",
                     ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-large-v2-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.json?download=true",
                     ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-large-v2-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.onnx?download=true"
                }
            },
            {"whisper-medium-int4",
                new HostedModel()
                {
                     DownloadSize = "715 MB",
                     ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-medium-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.json?download=true",
                     ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-medium-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.onnx?download=true"
                }
            },
            {"whisper-small-cpu-fp32",
                new HostedModel()
                {
                    DownloadSize = "1.14 GB",
                    ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-fp32/resolve/main/whisper_cpu_fp32_cpu-cpu_model.onnx?download=true",
                    ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-fp32/resolve/main/whisper_cpu_fp32_cpu-cpu_model.json?download=true",
                }
            },
            {"whisper-small-cpu-fp4",
                new HostedModel()
                {
                    DownloadSize = "467 MB",
                    ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-fp4/resolve/main/whisper_cpu_fp4_cpu-cpu_model.onnx?download=true",
                     ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-fp4/resolve/main/whisper_cpu_fp4_cpu-cpu_model.json?download=true"
                }
            },
            { "whisper-small-cpu-int4",
                new HostedModel()
                {
                    DownloadSize = "507 MB",
                     ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-int4/resolve/main/whisper_cpu_int4_cpu-cpu_model.onnx?download=true",
                     ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-int4/resolve/main/whisper_cpu_int4_cpu-cpu_model.json?download=true"
                }
            },
            {"whisper-small-cpu-int8",
                new HostedModel()
                {
                     DownloadSize = "444 MB",
                      ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-int8/resolve/main/whisper_cpu_int8_cpu-cpu_model.onnx?download=true",
                       ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-cpu-int8/resolve/main/whisper_cpu_int8_cpu-cpu_model.json?download=true"
                }
            },

            {"whisper-small-int4",
                new HostedModel()
                {
                    DownloadSize = "361 MB",
                    ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.onnx?download=true",
                    ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-small-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.json?download=true"
                }
            },
            {"whisper-tiny-int4",
                new HostedModel()
                {
                     DownloadSize = "112 MB",
                     ModelUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-tiny-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.onnx?download=true",
                     ModelConfigUrl = "https://huggingface.co/DzmitryShchamialiou/whisper-tiny-int4/resolve/main/whisper_gpu_int4_gpu-cuda_model.json?download=true"
                }
            }

        };

        public static List<string> ModelsName = [.. ModelsDictionary.Keys];

        private IHttpClientFactory httpClientFactory;
        public ModelManagerService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetModelPath(string modelName)
        {
            if (ModelsDictionary.TryGetValue(modelName, out var hostedModel))
            {
                var modelsFolder = Path.Combine(Environment.CurrentDirectory, "Models", modelName);

                if (!Directory.Exists(modelsFolder))
                {
                    Directory.CreateDirectory(modelsFolder);
                    await DownloadModelAsync(modelsFolder, hostedModel.ModelUrl, hostedModel.ModelConfigUrl, modelName);
                }

                return modelsFolder;
            }

            throw new NotSupportedException($"{modelName} model is not supported");
        }

        private async Task DownloadModelAsync(string modelFolder, string modelUrl, string modelConfigUrl, string modelName)
        {
            var client = httpClientFactory.CreateClient();

            await client.DownloadFileTaskAsync(new Uri(modelUrl), Path.Combine(modelFolder, $"{modelName}-model.onnx"));
            await client.DownloadFileTaskAsync(new Uri(modelConfigUrl), Path.Combine(modelFolder, $"{modelName}-model.json"));
        }

    }
}
