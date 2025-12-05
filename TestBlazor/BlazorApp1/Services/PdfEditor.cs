using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BackendLibrary
{
    public class PdfEditor
    {
        public static MemoryStream AddRect(PdfDocument pdf, int pageIndex, double x, double y, double width, double height, int dpiX, int dpiY)
        {
            MemoryStream mStream = new MemoryStream();
            using PdfDocument newPdf = new PdfDocument(mStream);
            // Create a new PDF page


            // for (int i = 0; i < pdf.PageCount; i++)
            // {
                    // newPdf.AddPage(pdf.Pages[i]);
            // }
            newPdf.AddPage(pdf.Pages[pageIndex]);
            
            PdfPage page = newPdf.Pages[0];

            // Create a graphics object for the page
            using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page))
            {
                // Create a rectangle
                // var rect = new PdfSharp.Drawing.XRect(x * dpiX, y * dpiY, width * dpiX, height * dpiY); // Convert inches to points

                var rect = PdfSharp.Drawing.XRect.FromLTRB(x * dpiX, y * dpiY, (x + width) * dpiX, (y + height) * dpiY);
                gfx.DrawRectangle(PdfSharp.Drawing.XPens.Pink, rect);
            }

            
            newPdf.Save(mStream);

            return mStream;
        }
    }
}