using BackendLibrary;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/20-NE0881-W001_P2.pdf"; 
        String filePath = "";
        //ShopTicket pdf = new ShopTicket(filePath);
        PdfDocument doc = PdfReader.Open(filePath,   PdfDocumentOpenMode.Import);
        String fileName = Path.GetFileNameWithoutExtension(filePath);

        //Console.WriteLine(pdf.ToString());
    /*    if(File.Exists(filePath))
        {
             Console.WriteLine(fileName);
        }
        else
        {
            Console.WriteLine("File not found ");
        }
    */    
       


        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}