using System.Text.RegularExpressions;

namespace BackendLibrary
{
    /// <summary>
    /// Utility class for ShopTicket.cs to extract FileName and FileNamePieceMark from PDF files.
    /// </summary>
    internal class PdfFileNameExtractor
    {
        public static string GetFileName(string pdfPath)
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

        public static string GetFileNamePieceMark(string fileName)
        {
            try
            {
                String NameOfFile = fileName;
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
                throw new ExtractionException("Failed to get FileNamePieceMark.", ex);
            }
        }
    }
}
