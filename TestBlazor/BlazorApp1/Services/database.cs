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
        private readonly string _dbPath;
        private readonly string _connectionString;

        public CustomerData(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            // Ensure DB lives under the content root so it's predictable
            _dbPath = Path.Combine(env.ContentRootPath, "ShopTicket.db");
            _connectionString = $"Data Source={_dbPath}";
        }

        public void CreateTables()
        {
            using (var connection = new SqliteConnection(_connectionString))
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
                        ControlNumbers TEXT,
                        PageNames TEXT,
                        Weight INTEGER,
                        FileContentPieceMark TEXT,
                        FileName TEXT UNIQUE,
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
                   ShopTicketID INTEGER UNIQUE,      
                   Foreign KEY (ShopTicketID) REFERENCES ShopTicket(ShopTicketID)

                     );
            ";

                command.ExecuteNonQuery();
            }
        }
        private const string ModelPath = "../../../../BackendLibrary/best.onnx";
        // private const string ModelPath = "C:/Users/critc/OneDrive/Desktop/Blobfish/Blobfish/BackendLibrary/best.onnx";

        public void AddDataToTables(ShopTicket pdf)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    // Prevent duplicate ShopTicket rows by FileName
                    var dupCheck = connection.CreateCommand();
                    dupCheck.CommandText = @"SELECT ShopTicketID FROM ShopTicket WHERE FileName = $fn LIMIT 1;";
                    dupCheck.Parameters.AddWithValue("$fn", pdf.FileName);
                    var existingTicket = dupCheck.ExecuteScalar();
                    if (existingTicket != null)
                    {
                        // Already inserted; skip all further inserts
                        return;
                    }

                    long newRecID;

                    // First, try to find an existing matching rectangle
                    var commandSelectRect = connection.CreateCommand();
                    commandSelectRect.CommandText =
                    @"
                    SELECT RecID FROM Rectangle
                    WHERE FormViewRectangleX = $fvrx
                      AND FormViewRectangleY = $fvry
                      AND FormViewRectangleWidth = $fvrw
                      AND FormViewRectangleHeight = $fvrh;
                    ";
                    commandSelectRect.Parameters.AddWithValue("$fvrx", pdf.FormViewRectangleX);
                    commandSelectRect.Parameters.AddWithValue("$fvry", pdf.FormViewRectangleY);
                    commandSelectRect.Parameters.AddWithValue("$fvrw", pdf.FormViewRectangleWidth);
                    commandSelectRect.Parameters.AddWithValue("$fvrh", pdf.FormViewRectangleHeight);

                    object? existingRecIDResult = commandSelectRect.ExecuteScalar();

                    if (existingRecIDResult != null)
                    {
                        // A matching rectangle already exists, use its ID
                        newRecID = (long)existingRecIDResult;
                    }
                    else
                    {
                        // No matching rectangle found, insert a new one
                        var commandInsertRect = connection.CreateCommand();
                        commandInsertRect.CommandText =
                        @"
                        INSERT INTO Rectangle (
                            FormViewRectangleX, FormViewRectangleY, FormViewRectangleWidth, FormViewRectangleHeight       
                        ) VALUES ($fvrx, $fvry, $fvrw, $fvrh);
                        SELECT last_insert_rowid();
                        ";
                        // Reuse the parameters defined above
                        commandInsertRect.Parameters.AddWithValue("$fvrx", pdf.FormViewRectangleX);
                        commandInsertRect.Parameters.AddWithValue("$fvry", pdf.FormViewRectangleY);
                        commandInsertRect.Parameters.AddWithValue("$fvrw", pdf.FormViewRectangleWidth);
                        commandInsertRect.Parameters.AddWithValue("$fvrh", pdf.FormViewRectangleHeight);

                        var insRectRes = commandInsertRect.ExecuteScalar();
                        if (insRectRes == null) throw new InvalidOperationException("Failed to retrieve new RecID.");
                        newRecID = Convert.ToInt64(insRectRes);
                    }

                    // 2. Insert data into the Child table (ShopTicket) using newRecID (FK)
                    var commandShop = connection.CreateCommand();
                    commandShop.CommandText =
                    @"
                INSERT INTO ShopTicket (
                    ProjectName, ProjectNumber, DesignNumber, PiecesRequired, ControlNumbers,PageNames, 
                    Weight, FileContentPieceMark, FileName, FileNamePieceMark, NumberOfPages, RecID
                ) VALUES ($pn, $prnu, $dn, $pire, $cn, $pnames, $we, $fcpm, $fn, $fnpm, $nop, $recid);
                SELECT last_insert_rowid();
                ";
                    commandShop.Parameters.AddWithValue("$pn", pdf.ProjectName);
                    commandShop.Parameters.AddWithValue("$prnu", pdf.ProjectNumber);
                    commandShop.Parameters.AddWithValue("$dn", pdf.DesignNumber);
                    commandShop.Parameters.AddWithValue("$pire", pdf.PiecesRequired);
                    commandShop.Parameters.AddWithValue("$cn", string.Join("   ", pdf.ControlNumbers != null ? string.Join("   ", pdf.ControlNumbers) : string.Empty));
                    commandShop.Parameters.AddWithValue("$pnames", string.Join("   ", pdf.PageNames));
                    commandShop.Parameters.AddWithValue("$we", pdf.Weight);
                    commandShop.Parameters.AddWithValue("$fcpm", pdf.FileContentPieceMark);
                    commandShop.Parameters.AddWithValue("$fn", pdf.FileName);
                    commandShop.Parameters.AddWithValue("$fnpm", (object?)pdf.FileNamePieceMark ?? DBNull.Value);
                    commandShop.Parameters.AddWithValue("$nop", pdf.NumberOfPages);
                    commandShop.Parameters.AddWithValue("$recid", newRecID);


                    var insShopRes = commandShop.ExecuteScalar();
                    if (insShopRes == null) throw new InvalidOperationException("Failed to retrieve new ShopTicketID.");
                    long newShopTicketID = Convert.ToInt64(insShopRes);


                    // 3. Insert data into the Project table using newShopTicketID (FK)
                    var commandProject = connection.CreateCommand();
                    commandProject.CommandText =
                    @"
                INSERT INTO Project (
                    ProjectName, DateCreated, ShopTicketID
                ) VALUES ($pn, $dc, $stid);
                ";
                    commandProject.Parameters.AddWithValue("$pn", pdf.ProjectName);
                    commandProject.Parameters.AddWithValue("$dc", pdf.dateTimeExtracted);
                    commandProject.Parameters.AddWithValue("$stid", newShopTicketID);

                    commandProject.ExecuteNonQuery();
                }
            }

            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
              
            }
            // General catch for any other unforeseen errors
            catch (Exception)
            {
                
            }
        }

        public List<ShopTicket> LoadTickets(ILoggerFactory loggerFactory)
        {
            var result = new List<ShopTicket>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT s.FileName, s.NumberOfPages, s.PageNames, s.FileNamePieceMark, s.ProjectNumber,
                       s.ProjectName, s.FileContentPieceMark, s.ControlNumbers, s.PiecesRequired,
                       s.Weight, s.DesignNumber,
                       r.FormViewRectangleX, r.FormViewRectangleY, r.FormViewRectangleWidth, r.FormViewRectangleHeight,
                       p.DateCreated
                FROM ShopTicket s
                LEFT JOIN Rectangle r ON s.RecID = r.RecID
                LEFT JOIN Project p ON p.ShopTicketID = s.ShopTicketID
                ORDER BY s.ShopTicketID ASC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string fileName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                int numberOfPages = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                string pageNamesRaw = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                string? fileNamePieceMark = reader.IsDBNull(3) ? null : reader.GetString(3);
                string projectNumber = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                string projectName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                string fileContentPieceMark = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                string controlNumbersRaw = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
                int piecesRequired = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
                decimal weight = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                string designNumber = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
                double rectX = reader.IsDBNull(11) ? 0 : reader.GetDouble(11);
                double rectY = reader.IsDBNull(12) ? 0 : reader.GetDouble(12);
                double rectW = reader.IsDBNull(13) ? 0 : reader.GetDouble(13);
                double rectH = reader.IsDBNull(14) ? 0 : reader.GetDouble(14);
                string dateCreatedRaw = reader.IsDBNull(15) ? string.Empty : reader.GetString(15);

                // Split helpers: values were joined with triple-spaces
                string[] pageNames = string.IsNullOrWhiteSpace(pageNamesRaw)
                    ? Array.Empty<string>()
                    : pageNamesRaw.Split(new[] { "   " }, StringSplitOptions.RemoveEmptyEntries);

                string[]? controlNumbers = string.IsNullOrWhiteSpace(controlNumbersRaw)
                    ? null
                    : controlNumbersRaw.Split(new[] { "   " }, StringSplitOptions.RemoveEmptyEntries);

                DateTime processed = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(dateCreatedRaw))
                {
                    DateTime.TryParse(dateCreatedRaw, out processed);
                }

                var ticket = new ShopTicket(
                    loggerFactory,
                    fileName,
                    numberOfPages,
                    pageNames,
                    fileNamePieceMark,
                    projectNumber,
                    projectName,
                    fileContentPieceMark,
                    controlNumbers,
                    piecesRequired,
                    weight,
                    designNumber,
                    0,
                    rectX,
                    rectY,
                    rectW,
                    rectH,
                    processed
                );

                result.Add(ticket);
            }
            return result;
        }

        // public void OpenExcelFile(string filePath)
        // {
        //     if (!File.Exists(filePath))
        //     {
        //         Console.WriteLine($"Error: Excel file not found at {filePath}");
        //         return;
        //     }

        //     try
        //     {
        //         Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        //         Console.WriteLine($"Opening {filePath}...");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"An error occurred while trying to open the file: {ex.Message}");
        //     }
        // }


        public void ExportToCsvShopTicket(string csvFilePath)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT ShopTicket.* FROM ShopTicket LEFT JOIN Rectangle ON ShopTicket.RecID = Rectangle.RecID";

                    using (var reader = command.ExecuteReader())
                    using (var writer = new StreamWriter(csvFilePath))
                    {

                        writer.WriteLine("ShopTicketID,ProjectName,ProjectNumber,DesignNumber,PiecesRequired,ContorlNumbers,PageNames,Weight,FileContentPieceMark,FileName,FileNamePieceMark,NumberOfPages,RecID");


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
                                reader.GetValue(10)?.ToString(),
                                reader.GetValue(11)?.ToString(),
                                reader.GetValue(12)?.ToString()
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
                using (var connection = new SqliteConnection(_connectionString))
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
                using (var connection = new SqliteConnection(_connectionString))
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

        public void RemoveRowByProjectID(int projectID)

        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int shopTicketID = 0;
                        int recID = 0;

                        // 1. Find the associated ShopTicketID from the Project table
                        using (var commandFindShopTicketId = connection.CreateCommand())
                        {
                            commandFindShopTicketId.CommandText = "SELECT ShopTicketID FROM Project WHERE ProjectID = $projectID";
                            commandFindShopTicketId.Parameters.AddWithValue("$projectID", projectID);
                            var result = commandFindShopTicketId.ExecuteScalar();

                            if (result is long longShopTicketId)
                            {
                                shopTicketID = (int)longShopTicketId;
                            }
                            else if (result is int intShopTicketId)
                            {
                                shopTicketID = intShopTicketId;
                            }
                            else
                            {
                                Console.WriteLine($"ProjectID {projectID} not found. Cannot proceed with deletion.");
                                transaction.Rollback();
                                return;
                            }
                        }

                        // 2. Delete the Project record itself (since we have the FK value now)
                        using (var commandDeleteProject = connection.CreateCommand())
                        {
                            commandDeleteProject.CommandText = "DELETE FROM Project WHERE ProjectID = $projectID";
                            commandDeleteProject.Parameters.AddWithValue("$projectID", projectID);
                            commandDeleteProject.ExecuteNonQuery();
                            Console.WriteLine($"Deleted Project record with ID {projectID}.");
                        }


                        // 3. Find the associated RecID from the ShopTicket table
                        using (var commandFindRecId = connection.CreateCommand())
                        {
                            commandFindRecId.CommandText = "SELECT RecID FROM ShopTicket WHERE ShopTicketID = $shopTicketID";
                            commandFindRecId.Parameters.AddWithValue("$shopTicketID", shopTicketID);
                            var result = commandFindRecId.ExecuteScalar();

                            if (result is long longRecId)
                            {
                                recID = (int)longRecId;
                            }
                            else if (result is int intRecId)
                            {
                                recID = intRecId;
                            }
                            // If recID is not found here, we can proceed but log a warning.
                        }


                        // 4. Delete the main record in the ShopTicket table
                        using (var commandDeleteShopTicket = connection.CreateCommand())
                        {
                            commandDeleteShopTicket.CommandText = "DELETE FROM ShopTicket WHERE ShopTicketID = $shopTicketID";
                            commandDeleteShopTicket.Parameters.AddWithValue("$shopTicketID", shopTicketID);
                            int rowsAffected = commandDeleteShopTicket.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Console.WriteLine($"Deleted ShopTicket record with ID {shopTicketID}.");
                            }
                        }

                        // 5. Delete the related record in the Rectangle table
                        if (recID > 0)
                        {
                            using (var commandDeleteRectangle = connection.CreateCommand())
                            {
                                commandDeleteRectangle.CommandText = "DELETE FROM Rectangle WHERE RecID = $recID";
                                commandDeleteRectangle.Parameters.AddWithValue("$recID", recID);
                                commandDeleteRectangle.ExecuteNonQuery();
                                Console.WriteLine($"Deleted associated Rectangle record with RecID {recID}.");
                            }
                        }

                        // Commit the transaction if all operations succeed
                        transaction.Commit();
                        Console.WriteLine($"Successfully removed all associated data starting from ProjectID {projectID}.");
                    }
                    catch (SqliteException ex)
                    {
                        Console.WriteLine($"A database error occurred: {ex.Message}. Rolling back operation.");
                        transaction.Rollback();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An unexpected error occurred: {ex.Message}. Rolling back operation.");
                        transaction.Rollback();
                    }
                }
            }
        }

        public int GetProjectIdFromUser()
        {
            Console.Write("Please enter the Project ID you wish to delete: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int projectId))
            {
                if (projectId > 0)
                {
                    return projectId;
                }
                else
                {
                    Console.WriteLine("Error: ID must be a positive number.");
                    return -1;
                }
            }
            else
            {
                Console.WriteLine("Error: Invalid input. Please enter a numerical ID.");
                return -1;
            }
        }
    }
}
