using BackendLibrary;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/20-NE0881-W001_P2.pdf";
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/24-NE1096-DT023_P0.pdf";
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/25-NE1203.01-W015_P2.pdf";
        String filePath = "C:/Users/lance/Documents/ShopTicketData/pdfs/20-NE0881-W050_P2.pdf";
        //String filePath = "";

        byte[] pdfBytes = File.ReadAllBytes(filePath);
        String pdfName = Path.GetFileNameWithoutExtension(filePath);
        ShopTicket pdf = new ShopTicket(pdfName, pdfBytes);

        if (File.Exists(filePath))
        {
            Console.WriteLine(pdf.ToString());
        }
        else
        {
            Console.WriteLine("File does not exist.");
        }

        

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}