namespace BackendLibrary
{
    public class PreloadService
    {
        // Dummy PDF Content
        private static byte[] _tinyPdf => File.ReadAllBytes("Services/tinypdf.pdf");

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

        private static void PreloadPdfPig()
        {     
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(_tinyPdf);
            _ = pdf.NumberOfPages;
            List<UglyToad.PdfPig.Content.Page> pages = pdf.GetPages().ToList();
            _ = pages[0].GetWords();
        }

        private static void PreloadPdfiumViewer()
        {
            MemoryStream stream = new MemoryStream(_tinyPdf);
            using var document = PdfiumViewer.PdfDocument.Load(stream);
            _ = document.PageCount;
            _ = document.Render(0, 72, 72, true);
        }

        private static void PreloadPdfSharp()
        {
            using var stream = new MemoryStream(_tinyPdf);
            using var document = PdfSharp.Pdf.IO.PdfReader.Open(stream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            _ = document.PageCount;
        }
    }
}
