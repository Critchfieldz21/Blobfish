using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.IO;

namespace BackendLibrary
{
    public class ShopTicket
    {
        public int NumberOfPages { get; set; }                              // Number of pages in the PDF file.
        //public string[] PageNames { get; set; }                    // Page names extracted from view labels.
        //public string FileName { get; set; }                       // File name of the PDF file.
        //public string? FileNamePieceMark { get; set; }                      // Piece Mark extracted from the file name.
        //public string ProjectNumber { get; set; }                  // Project Number from the title block labelled "JOB NO.".
        //public string ProjectName { get; set; }                    // Project Name from the title block labelled "PROJECT:".
        //public string FileContentPieceMark { get; set; }           // Piece Mark from the title block labelled "PIECE MARK".
        //public string[]? ControlNumbers { get; set; }                       // Control numbers from the square above the title block labelled "CONTROL NO.:".
        //public int PiecesRequired { get; set; }                    // Pieces required from the title block labelled "PIECES REQ'D:".
        //public decimal Weight { get; set; }                        // Weight from the title block labelled "WEIGHT:".
        //public string DesignNumber { get; set; }                   // Design number from the title block labelled "DESIGN:".
        //public int RectanglePage { get; set; }                              // 0-based index of the page containing form and section view rectangles.
        //public double FormViewRectangleX { get; set; }             // Distance from left edge of PDF to left edge of the form view rectangle (inches).
        //public double FormViewRectangleY { get; set; }             // Distance from top edge of PDF to top edge of the form view rectangle (inches).
        //public double FormViewRectangleWidth { get; set; }         // Width of the form view rectangle (inches).
        //public double FormViewRectangleHeight { get; set; }        // Height of the form view rectangle (inches).
        //public double SectionViewRectangleX { get; set; }          // Distance from left edge of PDF to left edge of the section view rectangle (inches).
        //public double SectionViewRectangleY { get; set; }          // Distance from top edge of PDF to top edge of the section view rectangle (inches).
        //public double SectionViewRectangleWidth { get; set; }      // Width of the section view rectangle (inches).
        //public double SectionViewRectangleHeight { get; set; }     // Height of the section view rectangle (inches).

        // test
        public ShopTicket()
        {
            NumberOfPages = 2;
        }
        public ShopTicket(String pdfPath)
        {
            // Use PdfSharp PdfReader to initialize a PdfDocument object off of the input file path
            PdfDocument pdf = PdfReader.Open(pdfPath);

            // Use PdfPig to extract text from pdf
            PdfTextExtractor pdfTextExtractor = new PdfTextExtractor(pdfPath);

            // OwnerPassword property needs a password to set SecuritySettings
            pdf.SecuritySettings.OwnerPassword = "admin";
            pdf.SecuritySettings.PermitModifyDocument = false;
            
            NumberOfPages = pdf.PageCount;
        }

        public override string ToString()
        {
            String str =
                "NumberOfPages: " + NumberOfPages + "\n";
            return str;
        }
    }
}