using BackendLibrary;

class Program
{
    static void Main(string[] args)
    {
        //string filePath = "25-NE1212-W044_P2.pdf";
        string filePath = "";
        ShopTicket pdf = new ShopTicket(filePath);
        Console.WriteLine(pdf.NumberOfPages);

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}