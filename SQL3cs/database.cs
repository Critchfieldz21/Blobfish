using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using  BackendLibrary;


namespace SQL3cs
{

    public class CustomerData
    {
        private const string DbFile = "customer.db";
        private static readonly string connectionString = $"Data Source={DbFile}";

        public class Customer
        {
            public int RowId { get; set; }
            public string NumberOfPages { get; set; }
            public string DesignNumber { get; set; }
            public string FileName { get; set; }
            public string FileNamePieceMark { get; set; }
            public string ProjectNumber { get; set; }
            public string ProjectName { get; set; }
            public string FileContentPieceMark { get; set; }
            public string Weight { get; set; }
            public string PiecesRequired { get; set; }
        }
        public void CreateCustomerTable()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS customer (
                    NumberOfPages TEXT,
                    DesignNumber TEXT,
                    FileName TEXT,
                    FileNamePieceMark TEXT,
                    ProjectNumber TEXT,
                    ProjectName TEXT,
                    FileContentPieceMark TEXT,
                    Weight TEXT,
                    PiecesRequired TEXT,
                    ByteArray TEXT               
                     );
            ";
                command.ExecuteNonQuery();
            }
        }

        //removes a row based in the row id 

        public void RemoveRow(int rowId)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM customer WHERE rowid = $id";
                    command.Parameters.AddWithValue("$id", rowId);
                    command.ExecuteNonQuery();
                }
                Console.WriteLine($"Row with RowId '{rowId}' successfully removed.");
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"An error occurred while removing the row: {ex.Message}");
            }
        }

        public void AddData(String filePath,  byte[] pdfBytes)
       {

            ShopTicket pdf = new ShopTicket(filePath, pdfBytes);
            byte[] bytes = pdfBytes;
            using (var connection = new SqliteConnection(connectionString))
            {
                
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO customer (
                    NumberOfPages, DesignNumber, FileName, FileNamePieceMark,
                    ProjectNumber, ProjectName, FileContentPieceMark, Weight, PiecesRequired, ByteArray
                ) VALUES ($nop, $pn, $fn, $fnpm, $pnum, $pname, $fcpm, $cn, $pr, $ba);
                ";

                // Assign hard-coded values directly to the parameters
                command.Parameters.AddWithValue("$nop", pdf.NumberOfPages.ToString());
                command.Parameters.AddWithValue("$pn",  pdf.DesignNumber);
                command.Parameters.AddWithValue("$fn", pdf.FileName);
                command.Parameters.AddWithValue("$fnpm", pdf.FileNamePieceMark);
                command.Parameters.AddWithValue("$pnum", pdf.ProjectNumber);
                command.Parameters.AddWithValue("$pname", pdf.ProjectName);
                command.Parameters.AddWithValue("$fcpm", pdf.FileContentPieceMark); 
                command.Parameters.AddWithValue("$cn", pdf.Weight.ToString());
                command.Parameters.AddWithValue("$pr", pdf.PiecesRequired.ToString());
                command.Parameters.AddWithValue("$ba", bytes.ToString());

                command.ExecuteNonQuery();
            }
        }

        // displays all rows in the database in a formatted table on the terminal
        public void ShowAll()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT rowid, * FROM customer";

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("The database is empty.");
                        return;
                    }

                    Console.WriteLine($"{"RowId",-5} {"Nop",-5} {"Pn",-5} {"Fn",-25} {"Fnpm",-10} " +
                                      $"{"Pnum",-15} {"Pname",-35} {"FCPM",-8} {"W",-5} {"Pr",-10}");

                    while (reader.Read())
                    {
                        Console.WriteLine(
                            $"{reader.GetInt64(0),-5} " +
                            $"{reader.GetString(1),-5} " +
                            $"{reader.GetString(2),-5} " +
                            $"{reader.GetString(3),-25} " +
                            $"{reader.GetString(4),-10} " +
                            $"{reader.GetString(5),-15} " +
                            $"{reader.GetString(6),-35} " +
                            $"{reader.GetString(7),-8} " +
                            $"{reader.GetString(8),-5} " +
                            $"{reader.GetString(9),-10}"
                        );
                    }
                }
            }
        }



        // reads a text file and outputs a csv file
        public void GetInfo()
        {
            string filePath = "/Users/zacharycritchfield/Desktop/DB/ShopTicketInfo.txt";
            string csvFilePath = "customers_data.csv";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found at {filePath}");
                return;
            }

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var writer = new StreamWriter(csvFilePath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string csvLine = line.Replace('|', ',');
                        writer.WriteLine(csvLine);
                    }
                }

                Console.WriteLine($"Successfully converted '{filePath}' to '{csvFilePath}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while converting the file: {ex.Message}");
            }
        }

        public void OpenExcelFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: Excel file not found at {filePath}");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                Console.WriteLine($"Opening {filePath}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while trying to open the file: {ex.Message}");
            }
        }


        public void RemoveRowFromInput()
        {
            ShowAll();
            Console.Write("Enter the RowId of the row to remove: ");
            if (int.TryParse(Console.ReadLine(), out int rowId))
            {
                RemoveRow(rowId);
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid RowId.");
            }
        }

        public void ExportToCsv(string csvFilePath)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM customer";

                    using (var reader = command.ExecuteReader())
                    using (var writer = new StreamWriter(csvFilePath))
                    {
                       
                        writer.WriteLine("NumberOfPages,DesignNumber,FileName,FileNamePieceMark,ProjectNumber,ProjectName,FileContentPieceMark,Weight,PiecesRequired");

                    
                        while (reader.Read())
                        {
                            var row = string.Join(",",
                                reader.GetString(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetString(4),
                                reader.GetString(5),
                                reader.GetString(6),
                                reader.GetString(7),
                                reader.GetString(8)
                            );
                            writer.WriteLine(row);
                        }
                    }
                }
                Console.WriteLine($"Data successfully exported to {csvFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while exporting data: {ex.Message}");
            }
        }
    }
}

