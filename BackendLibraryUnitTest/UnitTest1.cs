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
                },
                new object[] {
                    "../../../test_pdfs/24-NE1096-DT051_P0.pdf",    // filePath
                    2,                                              // expectedNumberOfPages
                    "24-NE1096-DT051_P0",                           // expectedFileName
                    "DT051",                                        // expectedFileNamePieceMark
                    "24-NE1096",                                    // expectedProjectNumber
                    "WOODBRIDGE METROPARK GARAGE",                  // expectedProjectName
                    "DT051",                                        // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    65800,                                          // expectedWeight
                    "DT1.00"                                        // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1193 -W017_P2.pdf",    // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1193 -W017_P2",                           // expectedFileName
                    "W017",                                         // expectedFileNamePieceMark
                    "25-NE1193",                                    // expectedProjectNumber
                    "EAST PARK 309",                                // expectedProjectName
                    "W017",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    50300,                                          // expectedWeight
                    "IWP2.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1200_W098 Rev.1_P2.pdf",// filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1200_W098 Rev.1_P2",                      // expectedFileName
                    "W098",                                         // expectedFileNamePieceMark
                    "25-NE1200",                                    // expectedProjectNumber
                    "YOURWAY PHARMA",                               // expectedProjectName
                    "W098",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    51500,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1203.02-W045_P2.pdf",  // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1203.02-W045_P2",                         // expectedFileName
                    "W045",                                         // expectedFileNamePieceMark
                    "25-NE1203.02",                                 // expectedProjectNumber
                    "SPECULATIVE OFFICE / WAREHOUSE B",             // expectedProjectName
                    "W045",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    35100,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1204-W080_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1204-W080_P2",                            // expectedFileName
                    "W080",                                         // expectedFileNamePieceMark
                    "25-NE1204",                                    // expectedProjectNumber
                    "LINK - HYATT",                                 // expectedProjectName
                    "W080",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    43900,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1204-W225_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1204-W225_P2",                            // expectedFileName
                    "W225",                                         // expectedFileNamePieceMark
                    "25-NE1204",                                    // expectedProjectNumber
                    "LINK - HYATT",                                 // expectedProjectName
                    "W225",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    52000,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1209-W016_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1209-W016_P2",                            // expectedFileName
                    "W016",                                         // expectedFileNamePieceMark
                    "25-NE1209",                                    // expectedProjectNumber
                    "PROJECT AERIE",                                // expectedProjectName
                    "W016",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    33200,                                          // expectedWeight
                    "IWP3.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1209-W207_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1209-W207_P2",                            // expectedFileName
                    "W207",                                         // expectedFileNamePieceMark
                    "25-NE1209",                                    // expectedProjectNumber
                    "PROJECT AERIE",                                // expectedProjectName
                    "W207",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    11100,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W109_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W109_P2",                            // expectedFileName
                    "W109",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W109",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    53396,                                          // expectedWeight
                    "SP1"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W266_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W266_P2",                            // expectedFileName
                    "W266",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W266",                                         // expectedFileContentPieceMark
                    1,                                              // expectedPiecesRequired
                    12361,                                          // expectedWeight
                    "SP3"                                           // expectedDesignNumber
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
