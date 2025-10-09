using BackendLibrary;

class Program
{
    static void Main(string[] args)
    {
        ShopTicket pdf = new ShopTicket();
        Console.WriteLine(pdf.NumberOfPages);
    }
}