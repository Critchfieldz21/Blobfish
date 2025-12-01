namespace BackendLibrary
{
    /// <summary>
    /// Represents extracted text from ShopTicket PDF content.
    /// </summary>
    public struct TextGroup
    {
        public string[] PageNames;
        public string ProjectNumber;
        public string ProjectName;
        public string FileContentPieceMark;
        public string[]? ControlNumbers;
        public int PiecesRequired;
        public decimal Weight;
        public string DesignNumber;
    }
}
