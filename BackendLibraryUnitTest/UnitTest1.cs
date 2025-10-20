using BackendLibrary;
using System.Runtime.CompilerServices;

namespace BackendLibraryUnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void CheckPdf1()
        {
            String filePath = "../../../test_pdfs/20-NE0881-W053_P2.pdf";
            byte[] pdfBytes = File.ReadAllBytes(filePath);
            String pdfName = Path.GetFileNameWithoutExtension(filePath);
            ShopTicket pdf = new ShopTicket(pdfName, pdfBytes);

            Assert.Multiple(
                () => Assert.Equal(3, pdf.NumberOfPages),
                () => Assert.Equal("20-NE0881-W053_P2", pdf.FileName),
                () => Assert.Equal("W053", pdf.FileNamePieceMark),
                () => Assert.Equal("20-NE0881", pdf.ProjectNumber),
                () => Assert.Equal("HRP HUDSON BUILDING 1", pdf.ProjectName),
                () => Assert.Equal("W053", pdf.FileContentPieceMark),
                () => Assert.Equal(1, pdf.PiecesRequired),
                () => Assert.Equal(44100, pdf.Weight),
                () => Assert.Equal("IWP1.00", pdf.DesignNumber));
        }
    }
}
