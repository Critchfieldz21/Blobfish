using BackendLibrary;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/20-NE0881-W001_P2.pdf";
        //String filePath = "";
         ShopTicket pdf = new ShopTicket(filePath);

        if (File.Exists(filePath))
        {
            Console.WriteLine($"File Name:          {pdf.GetFileName(filePath)}");
            Console.WriteLine($"File Piece Mark:    {pdf.FilePieceMarkark(filePath)}");
            Console.WriteLine($"Total Pages:        {pdf.NumberOfPages}");
            Console.WriteLine($"Project Number:     {pdf.ProjectNumber(filePath)}");
        }
        else
        {
            Console.WriteLine("File does not exist.");
        }

        
        

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}