namespace ShopTicketBackend
{
    public class ShopTicket
    {
        public int NumberOfPages { get; set; }                              // Number of pages in the PDF file.
        public required string[] PageNames { get; set; }                    // Page names extracted from view labels.
        public required string FileName { get; set; }                       // File name of the PDF file.
        public string? FileNamePieceMark { get; set; }                      // Piece Mark extracted from the file name.
        public required string ProjectNumber { get; set; }                  // Project Number from the title block labelled "JOB NO.".
        public required string ProjectName { get; set; }                    // Project Name from the title block labelled "PROJECT:".
        public required string FileContentPieceMark { get; set; }           // Piece Mark from the title block labelled "PIECE MARK".
        public string[]? ControlNumbers { get; set; }                       // Control numbers from the square above the title block labelled "CONTROL NO.:".
        public required int PiecesRequired { get; set; }                    // Pieces required from the title block labelled "PIECES REQ'D:".
        public required decimal Weight { get; set; }                        // Weight from the title block labelled "WEIGHT:".
        public required string DesignNumber { get; set; }                   // Design number from the title block labelled "DESIGN:".
        public int RectanglePage { get; set; }                              // 0-based index of the page containing form and section view rectangles.
        public required double FormViewRectangleX { get; set; }             // Distance from left edge of PDF to left edge of the form view rectangle (inches).
        public required double FormViewRectangleY { get; set; }             // Distance from top edge of PDF to top edge of the form view rectangle (inches).
        public required double FormViewRectangleWidth { get; set; }         // Width of the form view rectangle (inches).
        public required double FormViewRectangleHeight { get; set; }        // Height of the form view rectangle (inches).
        public required double SectionViewRectangleX { get; set; }          // Distance from left edge of PDF to left edge of the section view rectangle (inches).
        public required double SectionViewRectangleY { get; set; }          // Distance from top edge of PDF to top edge of the section view rectangle (inches).
        public required double SectionViewRectangleWidth { get; set; }      // Width of the section view rectangle (inches).
        public required double SectionViewRectangleHeight { get; set; }     // Height of the section view rectangle (inches).

        public ShopTicket()
        {

        }
    }
}
