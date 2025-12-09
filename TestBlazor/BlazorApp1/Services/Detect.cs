using SkiaSharp;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.Models;
using YoloDotNet.Extensions;
using System.Text.RegularExpressions;

namespace BackendLibrary
{
    public class Detect
    {
        // ⚠️ Note: The accuracy of inference results depends heavily on how you configure preprocessing and thresholds.
        // Make sure to read the README section "Accuracy Depends on Configuration":
        // https://github.com/NickSwardh/YoloDotNet/tree/master#%EF%B8%8F-accuracy-depends-on-configuration

        public static Rectangle GetRectInfo(ILogger<Detect> logger, string modelPath, string filePath)
        {
            System.IO.Directory.CreateDirectory(".\\temp\\");
            return ProcessImage(logger, modelPath, filePath, ".\\temp\\", 72, 72);
        }

        public static Rectangle GetRectInfo(ILogger<Detect> logger, string modelPath, string filePath, Stream pdffile)
        {
            System.IO.Directory.CreateDirectory(".\\temp\\");
            logger.LogDebug("Start processing image");
            return ProcessImage(logger, modelPath, filePath, pdffile, ".\\temp\\", 72, 72);
        }

        public static Rectangle ProcessImage(ILogger<Detect> logger, string modelPath, string imagePath, Stream pdffile, string outputFolder, float dpiX = 72, float dpiY = 72)
        {
            string input = Path.GetFileName(imagePath);
            string pattern = @"(?:_P)(\d+)";
            Match match = Regex.Match(input, pattern);
            int pageNumber = int.Parse(match.Groups[1].Value);
            // Implement image processing logic here
            float boxWidth = -1, boxHeight = -1, boxX = -1, boxY = -1;
            try
            {
                
                using var document = PdfiumViewer.PdfDocument.Load(pdffile);
                logger.LogDebug("Loaded PdfiumViewer PdfDocument");
                using var image = document.Render(pageNumber, dpiX, dpiY, true);
                logger.LogDebug("Rendered page {pageNumber} as image", pageNumber);

                // only supported for window
                string savedFilePath = Path.Combine(outputFolder, Path.GetFileName(imagePath)[..^4] + ".jpg");
                image.Save(savedFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                logger.LogDebug("Saved rendered image to {savedFilePath}", savedFilePath);
                //Console.WriteLine("Image dimensions: " + document.PageSizes[pageNumber]);
                var detectedBoundingBox = Detect.Detection(logger, modelPath, savedFilePath, savedFilePath);
                boxWidth = detectedBoundingBox.Width / dpiX;
                boxHeight = detectedBoundingBox.Height / dpiY;
                boxX = detectedBoundingBox.Left / dpiX;
                boxY = detectedBoundingBox.Top / dpiY;
                logger.LogDebug("Detected Bounding Box - X: {boxX}, Y: {boxY}, Width: {boxWidth}, Height: {boxHeight}", boxX, boxY, boxWidth, boxHeight);

            }

            catch (Exception ex)
            {
                //custom exception can be handled.
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return new Rectangle(pageNumber, boxX, boxY, boxWidth, boxHeight);
        }

        public static Rectangle ProcessImage(ILogger<Detect> logger, string modelPath, string imagePath, string outputFolder, float dpiX = 72, float dpiY = 72)
        {
            string input = Path.GetFileName(imagePath);
            string pattern = @"(?:_P)(\d+)";
            Match match = Regex.Match(input, pattern);
            int pageNumber = int.Parse(match.Groups[1].Value);
            // Implement image processing logic here
            float boxWidth = -1, boxHeight = -1, boxX = -1, boxY = -1;
            try
            {
                using var document = PdfiumViewer.PdfDocument.Load(imagePath);
                using var image = document.Render(pageNumber, dpiX, dpiY, true);

                // only supported for window
                string savedFilePath = Path.Combine(outputFolder, Path.GetFileName(imagePath)[..^4] + ".jpg");
                image.Save(savedFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                Console.WriteLine("Image processed and saved successfully.");
                Console.WriteLine("Image dimensions: " + document.PageSizes[pageNumber]);
                var detectedBoundingBox = Detect.Detection(logger, modelPath, savedFilePath, savedFilePath);
                boxWidth = detectedBoundingBox.Width / dpiX;
                boxHeight = detectedBoundingBox.Height / dpiY;
                boxX = detectedBoundingBox.Left / dpiX;
                boxY = detectedBoundingBox.Top / dpiY;
                Console.WriteLine($"Detected Bounding Box - X: {boxX}, Y: {boxY}, Width: {boxWidth}, Height: {boxHeight}");

            }

            catch (Exception ex)
            {
                //custom exception can be handled.
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return new Rectangle(pageNumber, boxX, boxY, boxWidth, boxHeight);
        }
        
        private static SKRectI Detection(ILogger<Detect> logger, string modelPath, string filePath, string outputPath)
        {

            //Console.WriteLine("Model Path: " + modelPath);
            using var yolo = new Yolo(new YoloOptions
            {
                OnnxModel = modelPath,

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
            });
            logger.LogDebug("Loaded YOLO model");

            // Load image using SkiaSharp
            using var image = SKBitmap.Decode(filePath);
            logger.LogDebug("Loaded image for object detection");

            // Run object detection
            DetectionDrawingOptions options = new DetectionDrawingOptions();
            options.DrawLabels = false;

            logger.LogDebug("Start detection");
            var results = yolo.RunObjectDetection(image, confidence: 0.20, iou: 0.7);
            

            if (results.Count == 0)
            {
                //Custom exception can be thrown here
                throw new Exception("No objects detected.");
            }
            return results[0].BoundingBox; //return the first bounding box
        }
    }
}