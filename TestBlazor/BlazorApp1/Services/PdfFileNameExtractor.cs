using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendLibrary
{
    internal class PdfFileNameExtractor
    {
        public String FileName { get; set; }

        private PdfFileNameExtractor()
        {

        }

        public static PdfFileNameExtractor InitializeWithPdfPath(String pdfPath)
        {
            PdfFileNameExtractor extractor = new PdfFileNameExtractor();
            extractor.FileName = extractor.GetFileName(pdfPath);
            return extractor;
        }

        public static PdfFileNameExtractor InitializeWithFileName(String fileName)
        {
            PdfFileNameExtractor extractor = new PdfFileNameExtractor();
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
                return "File name not found!";
            }

        }

        public String GetFileNamePieceMark()
        {
            String NameOfFile = FileName;
            char[] sep = { '-', '_' };

            String[] NameSplit = NameOfFile.Split(sep);
            return NameSplit[2];
        }
    }
}
