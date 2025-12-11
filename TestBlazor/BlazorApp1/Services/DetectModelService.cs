using Microsoft.Extensions.Logging;
using SkiaSharp;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.Models;

namespace BackendLibrary
{
    public class DetectModelService
    {
        public Yolo formViewModel { get; }
        public Yolo sectionViewModel { get; }

        public DetectModelService(ILogger<DetectModelService> logger, string formViewModelPath, string sectionViewModelPath)
        {
            YoloOptions options = new YoloOptions
            {
                OnnxModel = null,

                //ExecutionProvider = new CudaExecutionProvider(GpuId: 0, PrimeGpu: true),

                //using CPU for this case, so no need to download anything extra
                //   - CpuExecutionProvider         → CPU-only (no GPU required) 
                //   - CudaExecutionProvider        → GPU via CUDA (NVIDIA required)
                //   - TensorRtExecutionProvider    → GPU via NVIDIA TensorRT for maximum performance

                ImageResize = ImageResize.Proportional,

                // Proportional = the dataset images were not distorted; their aspect ratio was preserved.
                // Stretched = the dataset images were resized directly to the model's input size, ignoring aspect ratio.

                SamplingOptions = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None)

                // The choice of sampling method can directly affect detection accuracy, 
                // as different resampling methods (Nearest, Bilinear, Cubic, etc.) slightly alter object shapes and edges.
                // Check the benchmarks for examples and guidance: 
                // https://github.com/NickSwardh/YoloDotNet/tree/master/test/YoloDotNet.Benchmarks
            };

            options.OnnxModel = formViewModelPath;
            formViewModel = new Yolo(options);
            logger.LogDebug("Loaded YOLO model at {modelPath}", formViewModelPath);
            
            options.OnnxModel = sectionViewModelPath;
            sectionViewModel = new Yolo(options);
            logger.LogDebug("Loaded YOLO model at {modelPath}", sectionViewModelPath);
        }
    }
}
