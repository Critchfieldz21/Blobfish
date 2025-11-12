using BackendLibrary;
using SQL3cs;

class Program
{

    static void Main(string[] args)
    {

        // String filePath = "C:/Users/critc/OneDrive/Desktop/ShopTickets/pdfs/20-NE0881-W001_P2.pdf";
        // String filePath = "C:/Users/critc/OneDrive/Desktop/ShopTickets/pdfs/24-NE1096-DT023_P0.pdf";
        // String filePath = "C:/Users/critc/OneDrive/Desktop/ShopTickets/pdfs/25-NE1203.01-W015_P2.pdf";
        // String filePath = "C:/Users/critc/OneDrive/Desktop/ShopTickets/pdfs/24-NE1173-W023_P2.pdf";
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/20-NE0881-W001_P2.pdf";
        //String filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/24-NE1096-DT023_P0.pdf";
        //string filePath = "/Users/zacharycritchfield/Desktop/ShopTickets/ShopTicketData/pdfs/25-NE1203.01-W015_P2.pdf";
        // String filePath = "C:/Users/lance/Documents/ShopTicketData/pdfs/24-NE1173-W023_P2.pdf";
        String filePath = "";

        byte[] pdfBytes = File.ReadAllBytes(filePath);
        String pdfName = Path.GetFileNameWithoutExtension(filePath);
        ShopTicket pdf = new ShopTicket(pdfName, pdfBytes, "../../../../BackendLibrary/best.onnx");
        // ShopTicket pdf = new ShopTicket(pdfName, pdfBytes, "C:/Users/critc/OneDrive/Desktop/Blobfish/Blobfish/BackendLibrary/best.onnx");
        CustomerData db = new CustomerData();

        // ShopTicket pdf = new ShopTicket(filePath);

        if (File.Exists(filePath))
        {
            Console.WriteLine(pdf.ToString());

            db.CreateTables();

            db.AddDataToTables(pdf);

            // db.RemoveRowByProjectID(db.GetProjectIdFromUser());

            db.ExportToCsvShopTicket("../../../../BackendConsoleTest/ShopTicket_data.csv");
            db.ExportToCsvRectangle("../../../../BackendConsoleTest/rectangle_data.csv");
            db.ExportToCsvProject("../../../../BackendConsoleTest/project_data.csv");



        //     db.ExportToCsvShopTicket("C:/Users/critc/OneDrive/Desktop/Blobfish/Blobfish/BackendConsoleTest/ShopTicket_data.csv");
        //     db.ExportToCsvRectangle("C:/Users/critc/OneDrive/Desktop/Blobfish/Blobfish/BackendConsoleTest/rectangle_data.csv");
        //     db.ExportToCsvProject("C:/Users/critc/OneDrive/Desktop/Blobfish/Blobfish/BackendConsoleTest/project_data.csv");



                //     db.OpenExcelFile("C:/Users/critc/OneDrive/Desktop/Blobfish/Blobfish/BackendConsoleTest/ShopTicket_data.csv");
        }

        else
            {
                Console.WriteLine("File does not exist.");
            }



            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
