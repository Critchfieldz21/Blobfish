using System.Text.Json;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BackendLibrary
{
    /// <summary>
    /// A holder class representing a shop ticket extracted from a PDF file.
    /// </summary>
    public class ShopTicket
    {
        private readonly ILogger<ShopTicket> _logger;
        private readonly ILoggerFactory _loggerFactory;
        public byte[] PdfBytes { get; }                              // Bytearray of PDF file
        public DateTime dateTimeExtracted { get; }                   // Date and time when ShopTicket was constructed
        public int NumberOfPages { get; private set; }               // Number of pages in the PDF file.
        public string[] PageNames { get; private set; }              // Page names extracted from view labels.
        public string FileName { get; private set; }                 // File name of the PDF file.
        public string? FileNamePieceMark { get; private set; }       // Piece Mark extracted from the file name.
        public string ProjectNumber { get; private set; }            // Project Number from the title block labelled "JOB NO.".
        public string ProjectName { get; private set; }              // Project Name from the title block labelled "PROJECT:".
        public string FileContentPieceMark { get; private set; }     // Piece Mark from the title block labelled "PIECE MARK".
        public string[]? ControlNumbers { get; private set; }        // Control numbers from the square above the title block labelled "CONTROL NO.:".
        public int PiecesRequired { get; private set; }              // Pieces required from the title block labelled "PIECES REQ'D:".
        public decimal Weight { get; private set; }                  // Weight from the title block labelled "WEIGHT:".
        public string DesignNumber { get; private set; }             // Design number from the title block labelled "DESIGN:".
        public int RectanglePage { get; private set; }               // 0-based index of the page containing form and section view rectangles.
        public double FormViewRectangleX { get; private set; }       // Distance from left edge of PDF to left edge of the form view rectangle (inches).
        public double FormViewRectangleY { get; private set; }       // Distance from top edge of PDF to top edge of the form view rectangle (inches).
        public double FormViewRectangleWidth { get; private set; }   // Width of the form view rectangle (inches).
        public double FormViewRectangleHeight { get; private set; }  // Height of the form view rectangle (inches).
        //public double SectionViewRectangleX { get; set; }          // Distance from left edge of PDF to left edge of the section view rectangle (inches).
        //public double SectionViewRectangleY { get; set; }          // Distance from top edge of PDF to top edge of the section view rectangle (inches).
        //public double SectionViewRectangleWidth { get; set; }      // Width of the section view rectangle (inches).
        //public double SectionViewRectangleHeight { get; set; }     // Height of the section view rectangle (inches).

        /// <summary>
        /// ShopTicket constructor initializing from a PDF file path.
        /// </summary>
        public ShopTicket(ILoggerFactory loggerFactory, String pdfPath)
        {
            try
            {
                _loggerFactory = loggerFactory;
                _logger = _loggerFactory.CreateLogger<ShopTicket>();
                PdfBytes = File.ReadAllBytes(pdfPath);
                FileName = PdfFileNameExtractor.GetFileName(pdfPath);

                // Use PdfSharp PdfReader to initialize a PdfDocument object off of the input file path
                PdfDocument pdf = PdfReader.Open(pdfPath);

                InitializeFromPdf(pdf);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing shop ticket: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ShopTicket constructor initializing from PDF byte array.
        /// </summary>>
        public ShopTicket(ILoggerFactory loggerFactory, String fileName, byte[] pdfBytes)
        {
            try
            {
                _loggerFactory = loggerFactory;
                _logger = _loggerFactory.CreateLogger<ShopTicket>();
                PdfBytes = pdfBytes;
                FileName = fileName;

                // Use PdfSharp PdfReader to initialize a PdfDocument object off of pdf byte array
                MemoryStream stream = new MemoryStream(pdfBytes);
                PdfDocument pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                InitializeFromPdf(pdf);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error with {fileName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ShopTicket constructor initializing from PDF byte array with form view rectangle detection.
        /// </summary>
        public ShopTicket(ILoggerFactory loggerFactory, String fileName, byte[] pdfBytes, string modelPath)
        {
            try
            {
                _loggerFactory = loggerFactory;
                _logger = _loggerFactory.CreateLogger<ShopTicket>();
                _logger.LogDebug("Starting ShopTicket construction for {FileName}", fileName);
               
                PdfBytes = pdfBytes;
                FileName = fileName;

                // Use PdfSharp PdfReader to initialize a PdfDocument object off of pdf byte array
                MemoryStream stream = new MemoryStream(pdfBytes);
                _logger.LogDebug("MemoryStream created from PdfBytes");

                PdfDocument pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                _logger.LogDebug("PdfSharp PdfDocument created from stream");

                InitializeFromPdf(pdf);

                (RectanglePage, FormViewRectangleX, FormViewRectangleY, FormViewRectangleWidth, FormViewRectangleHeight) 
                    = Detect.GetRectInfo(_loggerFactory.CreateLogger<Detect>(), modelPath, fileName, stream);
                _logger.LogDebug("FormViewRectangle extracted");

                dateTimeExtracted = DateTime.Now;
                _logger.LogDebug("Ending ShopTicket construction for {FileName}", fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error with {fileName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Combine shared construction logic.
        /// </summary>
        private void InitializeFromPdf(PdfDocument pdf)
        {
            // OwnerPassword property needs a password to set SecuritySettings
            pdf.SecuritySettings.OwnerPassword = "admin";
            pdf.SecuritySettings.PermitModifyDocument = false;

            try
            {
                NumberOfPages = pdf.PageCount;
                _logger.LogDebug("NumberOfPages extracted");
            }
            catch (Exception ex)
            {
                throw new Exception("Error extracting basic PDF info", ex);
            }

            try
            {
                FileNamePieceMark = PdfFileNameExtractor.GetFileNamePieceMark(FileName);
                _logger.LogDebug("FileNamePieceMark extracted");

                TextGroup textGroup = PdfTextExtractor.GetExtractedText(_loggerFactory.CreateLogger<PdfTextExtractor>(), PdfBytes);
                _logger.LogDebug("TextGroup extracted");

                PageNames = textGroup.PageNames;
                ProjectNumber = textGroup.ProjectNumber;
                ProjectName = textGroup.ProjectName;
                FileContentPieceMark = textGroup.FileContentPieceMark;
                ControlNumbers = textGroup.ControlNumbers;
                PiecesRequired = textGroup.PiecesRequired;
                Weight = textGroup.Weight;
                DesignNumber = textGroup.DesignNumber;
            }
            catch (Exception ex)
            {
                throw new ExtractionException($"{FileName} has an extraction error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Override ToString to provide a readable representation of the ShopTicket.
        /// </summary>
        public override string ToString()
        {
            String str =
                dateTimeExtracted + "\n" +
                "NumberOfPages: " + NumberOfPages + "\n" +
                "PageNames: " + string.Join(", ", PageNames) + "\n" +
                "FileName: " + FileName + "\n" +
                "FileNamePieceMark: " + FileNamePieceMark + "\n" +
                "ProjectNumber: " + ProjectNumber + "\n" +
                "ProjectName: " + ProjectName + "\n" +
                "FileContentPieceMark: " + FileContentPieceMark + "\n" +
                "ControlNumbers: " + (ControlNumbers != null ? string.Join(", ", ControlNumbers) : "null") + "\n" +
                "PiecesRequired: " + PiecesRequired + "\n" +
                "Weight: " + Weight + " lb\n" +
                "RectanglePage: " + RectanglePage + "\n" +
                "DesignNumber: " + DesignNumber + "\n" +
                "FormViewRectangleX: " + FormViewRectangleX + "\n" +
                "FormViewRectangleY: " + FormViewRectangleY + "\n" +
                "FormViewRectangleWidth: " + FormViewRectangleWidth + "\n" +
                "FormViewRectangleHeight: " + FormViewRectangleHeight + "\n";

            return str;
        }

        /// <summary>
        /// Export the ShopTicket data to a JSON string.
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(ToExportDictionary(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        /// <summary>
        /// Export the ShopTicket data to a CSV string.
        /// </summary>
        public string ToCsv()
        {
            var dict = ToExportDictionary();
            var values = dict.Values.Select(v =>
            {
                if (v is not IEnumerable<string> list)
                {
                    // Join the list into one string
                    return v?.ToString()?.Replace(",", ";");
                }
                else
                {
                    return string.Join(";", list);
                }
            }); 
            // Replace commas in values to avoid breaking CSV
            var header = string.Join(",", dict.Keys);
            var row = string.Join(",", values);
            return $"{header}\n{row}";
        }

        /// <summary>
        /// Helper method to convert ShopTicket data to a dictionary for export.
        /// </summary>
        private Dictionary<string, object> ToExportDictionary()
        {
            return new Dictionary<string, object>
            {
                { "FileName", FileName },
                { "ProcessedDate", dateTimeExtracted },
                { "NumberOfPages", NumberOfPages },
                { "PageNames", PageNames },
                { "FileNamePieceMark", FileNamePieceMark },
                { "ProjectNumber", ProjectNumber },
                { "ProjectName", ProjectName },
                { "FileContentPieceMark", FileContentPieceMark },
                { "ControlNumbers", ControlNumbers },
                { "PiecesRequired", PiecesRequired },
                { "Weight", Weight },
                { "DesignNumber", DesignNumber },
                { "RectanglePage", RectanglePage },
                { "FormViewRectangleX", FormViewRectangleX },
                { "FormViewRectangleY", FormViewRectangleY },
                { "FormViewRectangleWidth", FormViewRectangleWidth },
                { "FormViewRectangleHeight", FormViewRectangleHeight }
            };
        }

        public void Info()
        {
            string str = NumberOfPages.ToString() + " | " + FileName + " | " + FileNamePieceMark + " | " + ProjectNumber +
                         " | " + ProjectName + " | " + FileContentPieceMark + " | " + PiecesRequired.ToString() +
                         " | " + Weight.ToString() + " lb | " + DesignNumber;

            String filepath = "/Users/zacharycritchfield/Documents/GitHub/Blobfish/SQL3cs/ShopTicketInfo.txt";

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
