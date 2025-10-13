using BackendLibrary;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/20-NE0881-W001_P2.pdf";
        //String filePath = "C:/Users/lance/Downloads/ShopTicketData/pdfs/24-NE1096-W029_P0.pdf";
        String filePath = "";

        ShopTicket pdf = new ShopTicket(filePath);

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