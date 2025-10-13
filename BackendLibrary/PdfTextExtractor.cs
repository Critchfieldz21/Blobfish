using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace BackendLibrary
{
    internal class PdfTextExtractor
    {
        private PdfDocument pdf;

        public PdfTextExtractor(String pdfPath)
        {
            pdf = PdfDocument.Open(File.OpenRead(pdfPath));
        }
    }
}
