using System.Text.RegularExpressions;

namespace BackendLibrary
{
    internal class PdfFileNameExtractor
    {
        public String FileName { get; set; }

        private PdfFileNameExtractor()
        {
            // Private constructor to enforce use of static constructor methods
        }

        public static PdfFileNameExtractor InitializeWithPdfPath(String pdfPath)
        {
            PdfFileNameExtractor extractor = new PdfFileNameExtractor();
            String fileName = extractor.GetFileName(pdfPath);
            if (fileName is null)
            {
                throw new Exception("FileName is null");
            }
            extractor.FileName = fileName;
            return extractor;
        }

        public static PdfFileNameExtractor InitializeWithFileName(String fileName)
        {
            PdfFileNameExtractor extractor = new PdfFileNameExtractor();
            if (fileName is null)
            {
                throw new Exception("FileName is null");
            }
            extractor.FileName = fileName;
            return extractor;
        }

        public String GetFileName(String pdfPath)
        {
            String filePath = pdfPath;

            String fileName = Path.GetFileNameWithoutExtension(filePath);

            if (File.Exists(filePath))
            {
                return fileName;
            }
            else
            {
                throw new FileNotFoundException($"File not found at path: {filePath}");
            }

        }

        public String GetFileNamePieceMark()
        {
            try
            {
                String NameOfFile = FileName;
                char[] sep = { '-', '_', ' ' };

                String[] NameSplit = NameOfFile.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                // Capture everything up to the last number
                string pattern = @"^(.*?\d+).*$";
                Match match = Regex.Match(NameSplit[2], pattern);

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                else
                {
                    return NameSplit[2];
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get FileNamePieceMark", ex);
            }
        }
    }
}
