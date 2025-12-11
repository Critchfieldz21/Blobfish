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

        public static Rectangle GetRectInfo(ILogger<Detect> logger, Yolo model, string filePath, Stream pdffile)
        {
            System.IO.Directory.CreateDirectory(".\\temp\\");
            logger.LogDebug("Start processing image");
            return ProcessImage(logger, model, filePath, pdffile, ".\\temp\\", 72, 72);
        }

        public static Rectangle ProcessImage(ILogger<Detect> logger, Yolo model, string imagePath, Stream pdffile, string outputFolder, float dpiX = 72, float dpiY = 72)
        {
            string input = Path.GetFileName(imagePath);
            string pattern = @"(?:_P)(\d+)";
            Match match = Regex.Match(input, pattern);
            int pageNumber = int.Parse(match.Groups[1].Value);
            // Implement image processing logic here
            float? boxWidth = null, boxHeight = null, boxX = null, boxY = null;
            try
            {
                
                var document = PdfiumViewer.PdfDocument.Load(pdffile);
                logger.LogDebug("Loaded PdfiumViewer PdfDocument");
                using var image = document.Render(pageNumber, dpiX, dpiY, true);
                logger.LogDebug("Rendered page {pageNumber} as image", pageNumber);

                // only supported for window
                string savedFilePath = Path.Combine(outputFolder, Path.GetFileName(imagePath)[..^4] + ".jpg");
                image.Save(savedFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                logger.LogDebug("Saved rendered image to {savedFilePath}", savedFilePath);
                //Console.WriteLine("Image dimensions: " + document.PageSizes[pageNumber]);
                var detectedBoundingBox = Detect.Detection(logger, model, savedFilePath, savedFilePath);
                boxWidth = detectedBoundingBox.Width / dpiX;
                boxHeight = detectedBoundingBox.Height / dpiY;
                boxX = detectedBoundingBox.Left / dpiX;
                boxY = detectedBoundingBox.Top / dpiY;
                logger.LogDebug("Detected Bounding Box - X: {boxX}, Y: {boxY}, Width: {boxWidth}, Height: {boxHeight}", boxX, boxY, boxWidth, boxHeight);

            }
            catch (DetectionException ex)
            {
                logger.LogInformation("Rectangle not found");
            }
            catch (Exception ex)
            {
                //custom exception can be handled.
                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }
            return new Rectangle(pageNumber, boxX, boxY, boxWidth, boxHeight);
        }
        
        private static SKRectI Detection(ILogger<Detect> logger, Yolo model, string filePath, string outputPath)
        {
            // Load image using SkiaSharp
            using var image = SKBitmap.Decode(filePath);
            logger.LogDebug("Loaded image for object detection");

            // Run object detection
            DetectionDrawingOptions options = new DetectionDrawingOptions();
            options.DrawLabels = false;

            logger.LogDebug("Start detection");

            if (model == null)
            {
                throw new Exception("YOLO model is not loaded.");
            }

            var results = model.RunObjectDetection(image, confidence: 0.20, iou: 0.7);
            
            if (results.Count == 0)
            {
                throw new DetectionException("No objects detected.");
            }

            File.Delete(filePath);
            return results[0].BoundingBox; //return the first bounding box
        }
    }
}