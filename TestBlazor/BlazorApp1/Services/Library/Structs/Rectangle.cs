namespace BackendLibrary
{
    /// <summary>
    /// Represents extracted rectangle information from ShopTicket PDF content.
    /// (ex: FormViewRectangle and SectionViewRectangle)
    /// </summary>
    public struct Rectangle
    {
        public int pageNumber;
        public float? boxX;
        public float? boxY;
        public float? boxWidth;
        public float? boxHeight;

        public Rectangle(int pageNumber, float? boxX, float? boxY, float? boxWidth, float? boxHeight)
        {
            this.pageNumber = pageNumber;
            this.boxX = boxX;
            this.boxY = boxY;
            this.boxWidth = boxWidth;
            this.boxHeight = boxHeight;
        }
    }
}
