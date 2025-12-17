namespace BackendLibrary
{
    /// <summary>
    /// Class for preloading PDF processing libraries to reduce first-use latency.
    /// </summary>
    public class PreloadService
    {
        /// <summary>
        /// Dummy PDF content.
        /// </summary>
        private static byte[] _tinyPdf => File.ReadAllBytes("Services/tinypdf.pdf");

        /// <summary>
        /// Preloads PDF processing libraries to reduce first-use latency.
        /// </summary>
        /// <param name="logger"></param>
        public static void PreloadPackages(ILogger<PreloadService> logger)
        {
            var jobs = new (Action Preload, string label)[]
            {
                (() =>  PreloadPdfPig(),            "PdfPig"),
                (() =>  PreloadPdfiumViewer(),      "PdfiumViewer"),
                (() =>  PreloadPdfSharp(),         "PdfSharp"),
            };

            Parallel.Invoke(
                jobs.Select(job => (Action)(() =>
                {
                    try
                    {
                        job.Preload();
                        logger.LogDebug("Preloaded {job.label} sucessfully.", job.label);
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug("Failed to load {job.label}: {ex.Message}.", job.label, ex.Message);
                    }
                }))
                .ToArray()
            );
        }

        /// <summary>
        /// Preloads PdfPig's PdfDocument Open, GetPages, and GetWords methods.
        /// </summary>
        private static void PreloadPdfPig()
        {     
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(_tinyPdf);
            _ = pdf.NumberOfPages;
            List<UglyToad.PdfPig.Content.Page> pages = pdf.GetPages().ToList();
            _ = pages[0].GetWords();
        }

        /// <summary>
        /// Preloads PdfiumViewer's PdfDocument Load, PageCount, and Render methods.
        /// </summary>
        private static void PreloadPdfiumViewer()
        {
            MemoryStream stream = new MemoryStream(_tinyPdf);
            using var document = PdfiumViewer.PdfDocument.Load(stream);
            _ = document.PageCount;
            _ = document.Render(0, 72, 72, true);
        }

        /// <summary>
        /// Preloads PdfSharp's PdfReader Open and PageCount methods.
        /// </summary>
        private static void PreloadPdfSharp()
        {
            using var stream = new MemoryStream(_tinyPdf);
            using var document = PdfSharp.Pdf.IO.PdfReader.Open(stream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            _ = document.PageCount;
        }
    }
}
