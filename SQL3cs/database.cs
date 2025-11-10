using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using  BackendLibrary;
using System.ComponentModel.Design;


namespace SQL3cs
{

    public class CustomerData
    {
        private const string DbFile = "ShopTicket.db";
        private static readonly string connectionString = $"Data Source={DbFile}";   
       
        public void CreateTables()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Rectangle (
                    RecID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FormViewRectangleX REAL,
                    FormViewRectangleY REAL,
                    FormViewRectangleWidth REAL,
                    FormViewRectangleHeight REAL        
                     );
            ";
             command.ExecuteNonQuery();

                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS ShopTicket (
                        ShopTicketID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProjectName TEXT,
                        ProjectNumber TEXT,
                        DesignNumber TEXT,
                        PiecesRequired INTEGER,
                        Weight INTEGER,
                        FileContentPieceMark TEXT,
                        FileName TEXT,
                        FileNamePieceMark TEXT,
                        NumberOfPages INTEGER,
                        RecID INTEGER,
                        FOREIGN KEY (RecID) REFERENCES Rectangle(RecID)
                    
                        );
                ";
                 command.ExecuteNonQuery();
                
                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Project (
                   ProjectID INTEGER PRIMARY KEY AUTOINCREMENT,
                   ProjectName TEXT,
                   DateCreated TEXT,
                   ShopTicketID INTEGER,      
                   Foreign KEY (ShopTicketID) REFERENCES ShopTicket(ShopTicketID)

                     );
            ";
                    
                command.ExecuteNonQuery();
            }
        }
             private const string ModelPath = "../../../../BackendLibrary/best.onnx";
        // private const string ModelPath = "C:/Users/critc/OneDrive/Desktop/Blobfish/Blobfish/BackendLibrary/best.onnx";

        public void AddDataToTables(String filePath)
        {
            ShopTicket pdf = new ShopTicket(filePath, File.ReadAllBytes(filePath), ModelPath);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // 1. Insert data into the PARENT table (Rectangle) and get its PK
                var commandRect = connection.CreateCommand();
                commandRect.CommandText =
                @"
                INSERT INTO Rectangle (
                    FormViewRectangleX, FormViewRectangleY, FormViewRectangleWidth, FormViewRectangleHeight       
                ) VALUES ($fvrx, $fvry, $fvrw, $fvrh);
                SELECT last_insert_rowid();
                ";
                commandRect.Parameters.AddWithValue("$fvrx", pdf.FormViewRectangleX);
                commandRect.Parameters.AddWithValue("$fvry", pdf.FormViewRectangleY);
                commandRect.Parameters.AddWithValue("$fvrw", pdf.FormViewRectangleWidth);
                commandRect.Parameters.AddWithValue("$fvrh", pdf.FormViewRectangleHeight);

                long newRecID = (long)commandRect.ExecuteScalar();


                // 2. Insert data into the Child table (ShopTicket) using newRecID (FK)
                var commandShop = connection.CreateCommand();
                commandShop.CommandText =
                @"
                INSERT INTO ShopTicket (
                    ProjectName, ProjectNumber, DesignNumber, PiecesRequired, 
                    Weight, FileContentPieceMark, FileName, FileNamePieceMark, NumberOfPages, RecID
                ) VALUES ($pn, $prnu, $dn, $pire, $we, $fcpm, $fn, $fnpm, $nop, $recid);
                
                SELECT last_insert_rowid(); -- Get the PK of the newly inserted ShopTicket
                ";
                commandShop.Parameters.AddWithValue("$pn", pdf.ProjectName);
                commandShop.Parameters.AddWithValue("$prnu", pdf.ProjectNumber);
                commandShop.Parameters.AddWithValue("$dn", pdf.DesignNumber);
                commandShop.Parameters.AddWithValue("$pire", pdf.PiecesRequired);
                commandShop.Parameters.AddWithValue("$we", pdf.Weight);
                commandShop.Parameters.AddWithValue("$fcpm", pdf.FileContentPieceMark);
                commandShop.Parameters.AddWithValue("$fn", pdf.FileName);
                commandShop.Parameters.AddWithValue("$fnpm", pdf.FileNamePieceMark);
                commandShop.Parameters.AddWithValue("$nop", pdf.NumberOfPages);
                commandShop.Parameters.AddWithValue("$recid", newRecID);


                long newShopTicketID = (long)commandShop.ExecuteScalar();


                // 3. Insert data into the Project table using newShopTicketID (FK)
                var commandProject = connection.CreateCommand();
                commandProject.CommandText =
                @"
                INSERT INTO Project (
                    ProjectName, DateCreated, ShopTicketID
                ) VALUES ($pn, $dc, $stid);
                ";
                commandProject.Parameters.AddWithValue("$pn", pdf.ProjectName);
                commandProject.Parameters.AddWithValue("$dc", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                commandProject.Parameters.AddWithValue("$stid", newShopTicketID);

                commandProject.ExecuteNonQuery();


                Console.WriteLine($"Inserted new Project linked to ShopTicket ID: {newShopTicketID}");
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

      public void ExportToCsvShopTicket(string csvFilePath)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT ShopTicket.* FROM ShopTicket LEFT JOIN Rectangle ON ShopTicket.RecID = Rectangle.RecID";

                    using (var reader = command.ExecuteReader())
                    using (var writer = new StreamWriter(csvFilePath))
                    {

                        writer.WriteLine("ShopTicketID,ProjectName,ProjectNumber,DesignNumber,PiecesRequired,Weight,FileContentPieceMark,FileName,FileNamePieceMark,NumberOfPages,RecID");


                        while (reader.Read())
                        {
                            var row = string.Join(",",
                            
                                reader.GetValue(0)?.ToString(),
                                reader.GetValue(1)?.ToString(),
                                reader.GetValue(2)?.ToString(),
                                reader.GetValue(3)?.ToString(),
                                reader.GetValue(4)?.ToString(),
                                reader.GetValue(5)?.ToString(),
                                reader.GetValue(6)?.ToString(),
                                reader.GetValue(7)?.ToString(),
                                reader.GetValue(8)?.ToString(),
                                reader.GetValue(9)?.ToString(),
                                reader.GetValue(10)?.ToString() 
                            );
                            writer.WriteLine(row);
                        }
                    }
                    
                    Console.WriteLine($"Data successfully exported to {csvFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during export: {ex.Message}");
            }
        }
    
        public void ExportToCsvRectangle(string csvFilePath)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Rectangle.* FROM Rectangle";

                    using (var reader = command.ExecuteReader())
                    using (var writer = new StreamWriter(csvFilePath))
                    {

                        writer.WriteLine("RecID,FormViewRectangleX,FormViewRectangleY,FormViewRectangleWidth,FormViewRectangleHeight");


                        while (reader.Read())
                        {
                            var row = string.Join(",",
                                reader.GetValue(0).ToString(),
                                reader.GetValue(1).ToString(),
                                reader.GetValue(2).ToString(),
                                reader.GetValue(3).ToString(),
                                reader.GetValue(4).ToString()

                            );
                            writer.WriteLine(row);
                        }
                    }

                    Console.WriteLine($"Data successfully exported to {csvFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while exporting data: {ex.Message}");
            }
        }
        public void ExportToCsvProject(string csvFilePath)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Project.* FROM Project";

                    using (var reader = command.ExecuteReader())
                    using (var writer = new StreamWriter(csvFilePath))
                    {
                       
                        writer.WriteLine("ProjectID,ProjectName,DateCreated,ShopTicketID");

                    
                        while (reader.Read())
                        {
                            var row = string.Join(",",
                                reader.GetValue(0).ToString(),
                                reader.GetValue(1).ToString(),
                                reader.GetValue(2).ToString(),
                                reader.GetValue(3).ToString()
                                

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

