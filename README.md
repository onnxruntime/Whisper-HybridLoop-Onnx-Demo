# Whisper-Hybrid-Loop-Onnx-Demo

This project demonstrates the use of Olive to optimize an AI model for specific hardware, utilizing the Whisper Tiny English version as a case study. The optimized model is then deployed to the device and using ONNX Runtime we can execute both local and cloud-based inference.

## Getting Started

### Prerequisites

Before beginning, ensure that you have the following:

1. Download Whisper-Tiny-Model from [here](https://huggingface.co/openai/whisper-tiny.en)
2. Follow the Olive tutorial for optimization [here](https://github.com/microsoft/Olive/tree/main/examples/whisper#whisper-optimization-using-ort-toolchain)
or
Dowload converted model from [here](https://huggingface.co/DzmitryShchamialiou)
3. Install Cuda 12.3 toolkit from [Nvidia](https://developer.nvidia.com/cuda-downloads?target_os=Windows&target_arch=x86_64)
4. Install Cudnn for cuda 12.3 from [Nvidia](https://developer.nvidia.com/rdp/cudnn-archive)

   Here a good example  [how to install Cuda and Cudnn](https://medium.com/analytics-vidhya/installing-cuda-and-cudnn-on-windows-d44b8e9876b5). In that example is legacy version but approach is the same.

### Installation

Clone the repository to your local machine with:

```sh
git clone https://github.com/onnxruntime/Whisper-HybridLoop-Onnx-Demo.git

```
or 
just download compiled version from Release.

## Optimization Setup

For detailed instructions on how to optimize the Whisper model for specific hardware using the Olive engine and Follows, please see our detailed [Tutorial on Model Optimization](https://github.com/microsoft/Olive/tree/main/examples/whisper#whisper-optimization-using-ort-toolchain).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
