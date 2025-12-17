using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BackendLibrary
{
    public class PdfEditor
    {
        public static byte[] AddRect(PdfDocument pdf, int pageIndex, double form_x, double form_y, double form_width, double form_height, double? section_x, double? section_y, double? section_width, double? section_height, int dpiX, int dpiY)
        {
            MemoryStream mStream = new MemoryStream();
            using PdfDocument newPdf = new PdfDocument(mStream);
            // Create a new PDF page


            for (int i = 0; i < pdf.PageCount; i++)
            {
                newPdf.AddPage(pdf.Pages[i]);
            }

            PdfPage page = newPdf.Pages[pageIndex];

            // Create a graphics object for the page
            using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page))
            {
                // Create a rectangle
                // var rect = new PdfSharp.Drawing.XRect(x * dpiX, y * dpiY, width * dpiX, height * dpiY); // Convert inches to points

                XPen pen = new XPen(XColors.Blue, 2);
                XPen SectionPen = new XPen(XColors.LawnGreen, 2);
                var form_rect = PdfSharp.Drawing.XRect.FromLTRB(form_x * dpiX, form_y * dpiY, (form_x + form_width) * dpiX, (form_y + form_height) * dpiY);
                gfx.DrawRectangle(pen, form_rect);
                //if section view does not exist
                if(section_x != null || section_y != null || section_width != null || section_height != null)
                {
                    var section_rect = PdfSharp.Drawing.XRect.FromLTRB((double)(section_x * dpiX), (double)(section_y * dpiY), (double)((section_x + section_width) * dpiX), (double)((section_y + section_height) * dpiY));
                    gfx.DrawRectangle(SectionPen, section_rect);
                }
            }

            
            newPdf.Save(mStream);

            return mStream.ToArray();
        }
    }
}