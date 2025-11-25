using System.Text;

namespace BackendLibrary
{
    public class PreloadService
    {
        // Dummy PDF Content
        private static byte[] _tinyPdf => Encoding.ASCII.GetBytes(
                @"%PDF-1.4
                1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj
                2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj
                3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] >> endobj
                xref
                0 4
                0000000000 65535 f 
                0000000010 00000 n 
                0000000060 00000 n 
                0000000110 00000 n 
                trailer << /Root 1 0 R /Size 4 >>
                startxref
                160
                %%EOF"
            );

        public static void PreloadPdfPig(ILogger<PreloadService> logger)
        {     
            try
            {
                using var pdf = UglyToad.PdfPig.PdfDocument.Open(_tinyPdf);
                _ = pdf.NumberOfPages;
                List<UglyToad.PdfPig.Content.Page> pages = pdf.GetPages().ToList();
                _ = pages[0].GetWords();
                logger.LogInformation("Preloaded PdfPig successfully.");
            }
            catch
            {
                logger.LogError("Failed to load PdfPig.");
            }
        }
    }
}
