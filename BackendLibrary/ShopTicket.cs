using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.IO;
using System.IO;
using System.IO.Enumeration;
using UglyToad.PdfPig.Graphics;

namespace BackendLibrary
{
    public class ShopTicket
    {
        public byte[] PdfBytes;
        public int NumberOfPages { get; set; }                              // Number of pages in the PDF file.
        //public string[] PageNames { get; set; }                    // Page names extracted from view labels.
        public string FileName { get; set; }                       // File name of the PDF file.
        public string? FileNamePieceMark { get; set; }                      // Piece Mark extracted from the file name.
        public string ProjectNumber { get; set; }                  // Project Number from the title block labelled "JOB NO.".
        public string ProjectName { get; set; }                    // Project Name from the title block labelled "PROJECT:".
        public string FileContentPieceMark { get; set; }           // Piece Mark from the title block labelled "PIECE MARK".
        //public string[]? ControlNumbers { get; set; }                       // Control numbers from the square above the title block labelled "CONTROL NO.:".
        public int PiecesRequired { get; set; }                    // Pieces required from the title block labelled "PIECES REQ'D:".
        public decimal Weight { get; set; }                        // Weight from the title block labelled "WEIGHT:".
        public string DesignNumber { get; set; }                   // Design number from the title block labelled "DESIGN:".
        //public int RectanglePage { get; set; }                              // 0-based index of the page containing form and section view rectangles.
        //public double FormViewRectangleX { get; set; }             // Distance from left edge of PDF to left edge of the form view rectangle (inches).
        //public double FormViewRectangleY { get; set; }             // Distance from top edge of PDF to top edge of the form view rectangle (inches).
        //public double FormViewRectangleWidth { get; set; }         // Width of the form view rectangle (inches).
        //public double FormViewRectangleHeight { get; set; }        // Height of the form view rectangle (inches).
        //public double SectionViewRectangleX { get; set; }          // Distance from left edge of PDF to left edge of the section view rectangle (inches).
        //public double SectionViewRectangleY { get; set; }          // Distance from top edge of PDF to top edge of the section view rectangle (inches).
        //public double SectionViewRectangleWidth { get; set; }      // Width of the section view rectangle (inches).
        //public double SectionViewRectangleHeight { get; set; }     // Height of the section view rectangle (inches).

        public ShopTicket(String pdfPath)
        {
            try
            {
                byte[] PdfBytes = File.ReadAllBytes(pdfPath);

                // Use PdfSharp PdfReader to initialize a PdfDocument object off of the input file path
                PdfDocument pdf = PdfReader.Open(pdfPath);

                // Use PdfPig to extract text from pdf
                PdfTextExtractor pdfTextExtractor = new PdfTextExtractor(pdfPath);

                InitializeFromPdf(pdf, pdfTextExtractor, PdfFileNameExtractor.InitializeWithPdfPath(pdfPath));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing shop ticket: {ex.Message}", ex);
            }
        }

        public ShopTicket(String fileName, byte[] pdfBytes)
        {
            try
            {
                PdfBytes = pdfBytes;

                // Use PdfSharp PdfReader to initialize a PdfDocument object off of pdf byte array
                MemoryStream stream = new MemoryStream(pdfBytes);
                PdfDocument pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                // Use PdfPig to extract text from pdf
                PdfTextExtractor pdfTextExtractor = new PdfTextExtractor(pdfBytes);
                InitializeFromPdf(pdf, pdfTextExtractor, PdfFileNameExtractor.InitializeWithFileName(fileName));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing shop ticket: {ex.Message}", ex);
            }
        }

        // Combine shared logic between constructors
        private void InitializeFromPdf(PdfDocument pdf, PdfTextExtractor pdfTextExtractor, PdfFileNameExtractor pdfFileNameExtractor)
        {
            // OwnerPassword property needs a password to set SecuritySettings
            pdf.SecuritySettings.OwnerPassword = "admin";
            pdf.SecuritySettings.PermitModifyDocument = false;

            try
            {
                NumberOfPages = pdf.PageCount;
                FileName = pdfFileNameExtractor.FileName;
            }
            catch (Exception ex)
            {
                throw new Exception("Error extracting basic PDF info", ex);
            }

            try
            {
                FileNamePieceMark = pdfFileNameExtractor.GetFileNamePieceMark();
                ProjectNumber = pdfTextExtractor.ExtractText("ProjectNumber");
                ProjectName = pdfTextExtractor.ExtractText("ProjectName");
                FileContentPieceMark = pdfTextExtractor.ExtractText("FileContentPieceMark");
                PiecesRequired = int.Parse(pdfTextExtractor.ExtractText("PiecesRequired"));
                Weight = decimal.Parse(pdfTextExtractor.ExtractText("Weight"));
                DesignNumber = pdfTextExtractor.ExtractText("DesignNumber");
            }
            catch (Exception ex)
            {
                throw new Exception($"{FileName} has an extraction error: {ex.Message}", ex);
            }
        }

        public override string ToString()
        {
            String str =
                "NumberOfPages: " + NumberOfPages + "\n" +
                "FileName: " + FileName + "\n" +
                "FileNamePieceMark: " + FileNamePieceMark + "\n" +
                "ProjectNumber: " + ProjectNumber + "\n" +
                "ProjectName: " + ProjectName + "\n" +
                "FileContentPieceMark: " + FileContentPieceMark + "\n" +
                "PiecesRequired: " + PiecesRequired + "\n" +
                "Weight: " + Weight + " lb\n" +
                "DesignNumber: " + DesignNumber + "\n";


            return str;
        }

        public void Info()
        {
            
            string str = NumberOfPages.ToString() + " | " + FileName + " | " + FileNamePieceMark+ " | " +ProjectNumber + " | " + ProjectName + " | " + FileContentPieceMark + " | " + PiecesRequired.ToString() + " | " + Weight.ToString() + " lb | " + DesignNumber;

            String filepath = "/Users/zacharycritchfield/Desktop/DB/ShopTicketInfo.txt";

            List<String> lines = new List<String>();
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
                lines.Add(str);
                File.WriteAllLines(filepath, lines);
            }
            else
            {
                lines.Add(str);
                File.WriteAllLines(filepath, lines);
            }
             

        }
    }
}