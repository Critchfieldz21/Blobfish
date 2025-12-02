using BackendLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System;
using Xunit.Abstractions;

namespace BackendLibraryTestingSuite
{
    public class ShopTicketIntegrationTest
    { 
        private static List<object[]> GetPdfTestDataFromCSV()
        {
            string testPdfsPath = "../../../test_files/test_pdf/";
            string testCsvPath = testPdfsPath + "test_pdf_data.csv";
            List<object[]> testData = new List<object[]>();

            using (TextFieldParser parser = new TextFieldParser(testCsvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                
                // Skip header line
                parser.ReadLine();
                
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields != null)
                    {
                        object[] dataRow = new object[12];
                        dataRow[0] = testPdfsPath + fields[0];                                      // filePath
                        dataRow[1] = int.Parse(fields[1]);                                          // expectedNumberOfPages
                        dataRow[2] = fields[2].Split(';');                                          // expectedPageNames
                        dataRow[3] = fields[3];                                                     // expectedFileName
                        dataRow[4] = fields[4];                                                     // expectedFileNamePieceMark
                        dataRow[5] = fields[5];                                                     // expectedProjectNumber
                        dataRow[6] = fields[6];                                                     // expectedProjectName
                        dataRow[7] = fields[7];                                                     // expectedFileContentPieceMark
                        dataRow[8] = string.IsNullOrEmpty(fields[8]) ? null : fields[8].Split(';'); // expectedControlNumbers
                        dataRow[9] = int.Parse(fields[9]);                                          // expectedPiecesRequired
                        dataRow[10] = decimal.Parse(fields[10]);                                    // expectedWeight
                        dataRow[11] = fields[11];                                                   // expectedDesignNumber
                        
                        testData.Add(dataRow);
                    }
                }
            }

            return testData;
        }

        private static List<object[]> GetPdfExceptionTestDataFromCSV()
        {
            string testPdfsPath = "../../../test_files/test_pdf_exceptions/";
            string testCsvPath = testPdfsPath + "test_pdf_exception_data.csv";
            List<object[]> testData = new List<object[]>();

            using (TextFieldParser parser = new TextFieldParser(testCsvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // Skip header line
                parser.ReadLine();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields != null)
                    {
                        object[] dataRow = new object[2];
                        dataRow[0] = testPdfsPath + fields[0];      // filePath
                        dataRow[1] = fields[1];                     // expectedException

                        testData.Add(dataRow);
                    }
                }
            }

            return testData;
        }

        public static IEnumerable<object[]> PdfTestData => GetPdfTestDataFromCSV();
        public static IEnumerable<object[]> PdfExceptionTestData => GetPdfExceptionTestDataFromCSV();
        public static ILoggerFactory _loggerFactory => LoggerFactory.Create(builder => builder.AddConsole());
        private readonly ITestOutputHelper _output;

        public ShopTicketIntegrationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(PdfTestData))]
        public void CheckPdf(
            String filePath, 
            int expectedNumberOfPages,
            String[] expectedPageNames,
            String expectedFileName, 
            String expectedFileNamePieceMark, 
            String expectedProjectNumber, 
            String expectedProjectName, 
            String expectedFileContentPieceMark,
            String[]? expectedControlNumbers,
            int expectedPiecesRequired, 
            decimal expectedWeight, 
            String expectedDesignNumber)
        {
            byte[] pdfBytes = File.ReadAllBytes(filePath);
            String pdfName = Path.GetFileNameWithoutExtension(filePath);
            ShopTicket pdf = new ShopTicket(_loggerFactory, pdfName, pdfBytes);

            Assert.Multiple(
                () => Assert.Equal(expectedNumberOfPages, pdf.NumberOfPages),
                () => Assert.Equal(expectedPageNames, pdf.PageNames),
                () => Assert.Equal(expectedFileName, pdf.FileName),
                () => Assert.Equal(expectedFileNamePieceMark, pdf.FileNamePieceMark),
                () => Assert.Equal(expectedProjectNumber, pdf.ProjectNumber),
                () => Assert.Equal(expectedProjectName, pdf.ProjectName),
                () => Assert.Equal(expectedFileContentPieceMark, pdf.FileContentPieceMark),
                () => Assert.Equal(expectedControlNumbers, pdf.ControlNumbers),
                () => Assert.Equal(expectedPiecesRequired, pdf.PiecesRequired),
                () => Assert.Equal(expectedWeight, pdf.Weight),
                () => Assert.Equal(expectedDesignNumber, pdf.DesignNumber)
            );
        }

        [Fact]
        public void CheckInvalidPdf()
        {
            byte[] invalidPdfBytes = System.Text.Encoding.UTF8.GetBytes("Invalid PDF content.");
            String pdfName = "invalid_pdf";
            Exception exception = Assert.Throws<Exception>(() => new ShopTicket(_loggerFactory, pdfName, invalidPdfBytes));
            _output.WriteLine($"Exception: {exception.Message}");

            Assert.Multiple(
                () => Assert.Contains("The file is not a valid PDF document", exception.Message),
                () => Assert.Contains(pdfName, exception.Message)
            );
        }

        [Theory]
        [MemberData(nameof(PdfExceptionTestData))]
        public void CheckPdfException(string filePath, string expectedException)
        {
            byte[] pdfBytes = File.ReadAllBytes(filePath);
            String pdfName = Path.GetFileNameWithoutExtension(filePath);
            _output.WriteLine($"pdfName: {pdfName}");
            Exception exception = Assert.Throws<Exception>(() => new ShopTicket(_loggerFactory, pdfName, pdfBytes));
            _output.WriteLine($"Exception: {exception.Message}");

            Assert.Equal(expectedException, exception.Message);
        }
           
    }
}
