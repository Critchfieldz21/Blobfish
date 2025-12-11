using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BackendLibrary
{
    /// <summary>
    /// A holder class representing a shop ticket extracted from a PDF file.
    /// </summary>
    public class ShopTicket
    {
        /// <summary>
        /// Used for constructing loggers in different classes.
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Logs messages for this class.
        /// </summary>
        private readonly ILogger<ShopTicket> _logger;

        /// <summary>
        /// Bytearray of PDF file.
        /// </summary>
        public byte[] PdfBytes { get; }

        /// <summary>
        /// Date and time when ShopTicket was constructed.
        /// </summary>
        public DateTime dateTimeExtracted { get; }

        /// <summary>
        /// Number of pages in the PDF file.
        /// </summary>
        public int NumberOfPages { get; private set; }

        /// <summary>
        /// Page names extracted from view labels.
        /// </summary>
        public string[] PageNames { get; private set; }

        /// <summary>
        /// File name of the PDF file.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Piece Mark extracted from the file name.
        /// </summary>
        public string? FileNamePieceMark { get; private set; }

        /// <summary>
        /// Project Number from the title block labelled "JOB NO.".
        /// </summary>
        public string ProjectNumber { get; private  set; }

        /// <summary>
        /// Project Name from the title block labelled "PROJECT:".
        /// </summary>
        public string ProjectName { get; private set; }

        /// <summary>
        /// Piece Mark from the title block labelled "PIECE MARK".
        /// </summary>
        public string FileContentPieceMark { get; private set; }

        /// <summary>
        /// Control numbers from the square above the title block labelled "CONTROL NO.:".
        /// </summary>
        public string[]? ControlNumbers { get; private set; }

        /// <summary>
        /// Pieces required from the title block labelled "PIECES REQ'D:".
        /// </summary>
        public int PiecesRequired { get; private set; }

        /// <summary>
        /// Weight from the title block labelled "WEIGHT:".
        /// </summary>
        public decimal Weight { get; private set; }

        /// <summary>
        /// Design number from the title block labelled "DESIGN:".
        /// </summary>
        public string DesignNumber { get; private set; }

        /// <summary>
        /// 0-based index of the page containing form and section view rectangles.
        /// </summary>
        public int RectanglePage { get; private set; }

        /// <summary>
        /// Distance from left edge of PDF to left edge of the form view rectangle (inches).
        /// </summary>
        public double FormViewRectangleX { get; private set; }

        /// <summary>
        /// Distance from top edge of PDF to top edge of the form view rectangle (inches).
        /// </summary>
        public double FormViewRectangleY { get; private set; }

        /// <summary>
        /// Width of the form view rectangle (inches).
        /// </summary>
        public double FormViewRectangleWidth { get; private set; }

        /// <summary>
        /// Height of the form view rectangle (inches).
        /// </summary>
        public double FormViewRectangleHeight { get; private set; }

        /// <summary>
        /// Distance from left edge of PDF to left edge of the section view rectangle (inches).
        /// </summary>
        public double? SectionViewRectangleX { get; private set; }

        /// <summary>
        /// Distance from top edge of PDF to top edge of the section view rectangle (inches).
        /// </summary>
        public double? SectionViewRectangleY { get; private set; }

        /// <summary>
        /// Width of the section view rectangle (inches).
        /// </summary>
        public double? SectionViewRectangleWidth { get; private set; }

        /// <summary>
        /// Height of the section view rectangle (inches).
        /// </summary>
        public double? SectionViewRectangleHeight { get; private set; }

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
        public ShopTicket(ILoggerFactory loggerFactory, String fileName, byte[] byteArray)
        {
            try
            {
                _loggerFactory = loggerFactory;
                _logger = _loggerFactory.CreateLogger<ShopTicket>();
                PdfBytes = byteArray;
                FileName = fileName;

                // Use PdfSharp PdfReader to initialize a PdfDocument object off of pdf byte array
                MemoryStream stream = new MemoryStream(byteArray);
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
        public ShopTicket(ILoggerFactory loggerFactory, String fileName, byte[] byteArray, DetectModelService dMService)
        {
            try
            {
                _loggerFactory = loggerFactory;
                _logger = _loggerFactory.CreateLogger<ShopTicket>();
                _logger.LogDebug("Starting ShopTicket construction for {FileName}", fileName);
               
                PdfBytes = byteArray;
                FileName = fileName;

                // Use PdfSharp PdfReader to initialize a PdfDocument object off of pdf byte array
                MemoryStream stream = new MemoryStream(byteArray);
                _logger.LogDebug("MemoryStream created from PdfBytes");

                PdfDocument pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                _logger.LogDebug("PdfSharp PdfDocument created from stream");

                InitializeFromPdf(pdf);

                Rectangle FormViewRectangle = Detect.GetRectInfo(_loggerFactory.CreateLogger<Detect>(), dMService.formViewModel, fileName, stream);
                RectanglePage = FormViewRectangle.pageNumber;
                FormViewRectangleX = (double) FormViewRectangle.boxX;
                FormViewRectangleY = (double) FormViewRectangle.boxY;
                FormViewRectangleWidth = (double) FormViewRectangle.boxWidth;
                FormViewRectangleHeight = (double) FormViewRectangle.boxHeight;
                _logger.LogDebug("FormViewRectangle extracted");

                Rectangle SectionViewRectangle = Detect.GetRectInfo(_loggerFactory.CreateLogger<Detect>(), dMService.sectionViewModel, fileName, stream);
                RectanglePage = SectionViewRectangle.pageNumber;
                SectionViewRectangleX = SectionViewRectangle.boxX;
                SectionViewRectangleY = SectionViewRectangle.boxY;
                SectionViewRectangleWidth = SectionViewRectangle.boxWidth;
                SectionViewRectangleHeight = SectionViewRectangle.boxHeight;
                _logger.LogDebug("SectionViewRectangle extracted");

                PdfBytes = PdfEditor.AddRect(
                    pdf, 
                    RectanglePage, 
                    FormViewRectangleX, FormViewRectangleY, FormViewRectangleWidth, FormViewRectangleHeight, 
                    SectionViewRectangleX, SectionViewRectangleY, SectionViewRectangleWidth, SectionViewRectangleHeight, 72, 72);
                _logger.LogDebug("Added form view and section view rectangles to PDF page {RectanglePage}", RectanglePage);


                dateTimeExtracted = DateTime.Now;
                _logger.LogDebug("Ending ShopTicket construction for {FileName}", fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error with {fileName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ShopTicket constructor initializing directly from stored database fields
        /// </summary>
        public ShopTicket(
            ILoggerFactory loggerFactory,
            byte[] pdfBytes,
            string fileName,
            int numberOfPages,
            string[] pageNames,
            string? fileNamePieceMark,
            string projectNumber,
            string projectName,
            string fileContentPieceMark,
            string[]? controlNumbers,
            int piecesRequired,
            decimal weight,
            string designNumber,
            int rectanglePage,
            double formViewRectangleX,
            double formViewRectangleY,
            double formViewRectangleWidth,
            double formViewRectangleHeight,
            double? sectionViewRectangleX,
            double? sectionViewRectangleY,
            double? sectionViewRectangleWidth,
            double? sectionViewRectangleHeight,
            DateTime processedDate)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ShopTicket>();

            PdfBytes = pdfBytes;
            FileName = fileName;
            NumberOfPages = numberOfPages;
            PageNames = pageNames;
            FileNamePieceMark = fileNamePieceMark;
            ProjectNumber = projectNumber;
            ProjectName = projectName;
            FileContentPieceMark = fileContentPieceMark;
            ControlNumbers = controlNumbers;
            PiecesRequired = piecesRequired;
            Weight = weight;
            DesignNumber = designNumber;
            RectanglePage = rectanglePage;
            FormViewRectangleX = formViewRectangleX;
            FormViewRectangleY = formViewRectangleY;
            FormViewRectangleWidth = formViewRectangleWidth;
            FormViewRectangleHeight = formViewRectangleHeight;
            SectionViewRectangleX = sectionViewRectangleX;
            SectionViewRectangleY = sectionViewRectangleY;
            SectionViewRectangleWidth = sectionViewRectangleWidth;
            SectionViewRectangleHeight = sectionViewRectangleHeight;
            dateTimeExtracted = processedDate;

            _logger.LogDebug("ShopTicket loaded for {FileName}", fileName);
        }

        /// <summary>
        /// Combine shared construction logic.
        /// </summary>
        // Supress warnings about nullable properties being uninitialized
        [MemberNotNull(nameof(PageNames), nameof(ProjectNumber), nameof(ProjectName), nameof(FileContentPieceMark), nameof(DesignNumber))]
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

                if (FileNamePieceMark != FileContentPieceMark)
                {
                    throw new ExtractionException($"Piece Mark mismatch: FileNamePieceMark '{FileNamePieceMark}' does not match FileContentPieceMark '{FileContentPieceMark}'");
                }
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
            return JsonSerializer.Serialize(ToExportDictionary(false), new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        /// <summary>
        /// Export the ShopTicket data to a CSV string.
        /// </summary>
        public string ToCsv()
        {
            var dict = ToExportDictionary(false);
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
        /// Used specifically for display on web page.
        /// </summary>
        public Dictionary<string, object> ToExportDictionary(Boolean roundValues)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "FileName", FileName },
                { "ProcessedDate", dateTimeExtracted },
                { "NumberOfPages", NumberOfPages },
                { "PageNames", PageNames != null ? string.Join(", ", PageNames) : "" },
                { "FileNamePieceMark", FileNamePieceMark ?? string.Empty },
                { "ProjectNumber", ProjectNumber },
                { "ProjectName", ProjectName },
                { "FileContentPieceMark", FileContentPieceMark },
                { "ControlNumbers", ControlNumbers != null ? string.Join(", ", ControlNumbers) : "" },
                { "PiecesRequired", PiecesRequired },
                { "Weight", Weight },
                { "DesignNumber", DesignNumber },
                { "RectanglePage", RectanglePage },
                { "FormViewRectangleX", FormViewRectangleX },
                { "FormViewRectangleY", FormViewRectangleY },
                { "FormViewRectangleWidth", FormViewRectangleWidth },
                { "FormViewRectangleHeight", FormViewRectangleHeight }
            };           
            if (!roundValues)
            {
                dict.Add("SectionViewRectangleX", SectionViewRectangleX != null ? SectionViewRectangleX : "");
                dict.Add("SectionViewRectangleY", SectionViewRectangleY != null ? SectionViewRectangleY : "");
                dict.Add("SectionViewRectangleWidth", SectionViewRectangleWidth != null ? SectionViewRectangleWidth : "");
                dict.Add("SectionViewRectangleHeight", SectionViewRectangleHeight != null ? SectionViewRectangleHeight : "");
            }
            else
            {
                dict.Add("SectionViewRectangleX", SectionViewRectangleX != null ? Double.Round((double)SectionViewRectangleX, 4) : "");
                dict.Add("SectionViewRectangleY", SectionViewRectangleY != null ? Double.Round((double)SectionViewRectangleY, 4) : "");
                dict.Add("SectionViewRectangleWidth", SectionViewRectangleWidth != null ? Double.Round((double)SectionViewRectangleWidth, 4) : "");
                dict.Add("SectionViewRectangleHeight", SectionViewRectangleHeight != null ? Double.Round((double)SectionViewRectangleHeight, 4) : "");
            }
            return dict;
        }
    }
}
