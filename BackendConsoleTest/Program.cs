using BackendLibrary;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Change file path accordingly based on your system
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/20-NE0881-W001_P2.pdf";
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/24-NE1096-DT023_P0.pdf";
        //string filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/25-NE1203.01-W015_P2.pdf";
        String filePath = "C:/Users/lance/Source/Repos/Blobfish/BackendLibraryUnitTest/test_pdfs/25-NE1203.02-W045_P2.pdf";
        //String filePath = "";

        byte[] pdfBytes = File.ReadAllBytes(filePath);
        String pdfName = Path.GetFileNameWithoutExtension(filePath);
        ShopTicket pdf = new ShopTicket(pdfName, pdfBytes);

        //ShopTicket pdf = new ShopTicket(filePath);

        if (File.Exists(filePath))
        {
            Console.WriteLine(pdf.ToString());
            //pdf.Info();
        }
        else
        {
            Console.WriteLine("File does not exist.");
        }

        

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}