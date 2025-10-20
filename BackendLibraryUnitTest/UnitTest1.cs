using BackendLibrary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BackendLibraryUnitTest
{
    public class UnitTest1
    {
        public static IEnumerable<object[]> TestData =>
            new List<object[]>
            {
                // Add PDF to test here
                new object[] {
                    "../../../test_pdfs/20-NE0881-W053_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W053_P2",                            // expectedFileName
                    "W053",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W053",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    44100,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W099_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W099_P2",                            // expectedFileName
                    "W099",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W099",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    54700,                                          // expectedWeight
                    "IWP2.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W184_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W184_P2",                            // expectedFileName
                    "W184",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W184",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    44000,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1087-W003_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "24-NE1087-W003_P2",                            // expectedFileName
                    "W003",                                         // expectedFileNamePieceMark
                    "24-NE1087",                                    // expectedProjectNumber
                    "TNTR CLUBHOUSE",                               // expectedProjectName
                    "W003",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    21400,                                          // expectedWeight
                    "IWP3.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1087-W093_P1.pdf",     // filePath
                    2,                                              // expectedNumberOfPages
                    "24-NE1087-W093_P1",                            // expectedFileName
                    "W093",                                         // expectedFileNamePieceMark
                    "24-NE1087",                                    // expectedProjectNumber
                    "TNTR CLUBHOUSE",                               // expectedProjectName
                    "W093",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    11000,                                          // expectedWeight
                    "WP2.00"                                        // expectedDesignNumber
                }
            };

        [Theory]
        [MemberData(nameof(TestData))]
        public void CheckPdf(
            String filePath, 
            int expectedNumberOfPages, 
            String expectedFileName, 
            String expectedFileNamePieceMark, 
            String expectedProjectNumber, 
            String expectedProjectName, 
            String expectedFileContentPieceMark, 
            int expectedPiecesRequired, 
            int expectedWeight, 
            String expectedDesignNumber)
        {
            byte[] pdfBytes = File.ReadAllBytes(filePath);
            String pdfName = Path.GetFileNameWithoutExtension(filePath);
            ShopTicket pdf = new ShopTicket(pdfName, pdfBytes);

            Assert.Multiple(
                () => Assert.Equal(expectedNumberOfPages, pdf.NumberOfPages),
                () => Assert.Equal(expectedFileName, pdf.FileName),
                () => Assert.Equal(expectedFileNamePieceMark, pdf.FileNamePieceMark),
                () => Assert.Equal(expectedProjectNumber, pdf.ProjectNumber),
                () => Assert.Equal(expectedProjectName, pdf.ProjectName),
                () => Assert.Equal(expectedFileContentPieceMark, pdf.FileContentPieceMark),
                () => Assert.Equal(expectedPiecesRequired, pdf.PiecesRequired),
                () => Assert.Equal(expectedWeight, pdf.Weight),
                () => Assert.Equal(expectedDesignNumber, pdf.DesignNumber));
        }
    }
}
