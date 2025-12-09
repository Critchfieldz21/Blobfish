using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using BackendLibrary;
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
                        RectanglePage INTEGER,
                        FormViewRectangleX REAL,
                        FormViewRectangleY REAL,
                        FormViewRectangleWidth REAL,
                        FormViewRectangleHeight REAL
                        SectionViewRectangleX REAL,
                        SectionViewRectangleY REAL,
                        SectioniewRectangleWidth REAL,
                        SectionViewRectangleHeight REAL
                             
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
                        PdfBlob BLOB,
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

        public void AddDataToTables(ShopTicket pdf)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    // If a ticket with the same piece mark exists, remove the old and keep the new
                    if (!string.IsNullOrWhiteSpace(pdf.FileNamePieceMark))
                    {
                        var findByPm = connection.CreateCommand();
                        findByPm.CommandText = @"SELECT ShopTicketID, RecID FROM ShopTicket WHERE FileNamePieceMark = $pm LIMIT 1;";
                        findByPm.Parameters.AddWithValue("$pm", pdf.FileNamePieceMark);
                        using var pmReader = findByPm.ExecuteReader();
                        int existingShopId = 0; long existingRecId = 0;
                        if (pmReader.Read())
                        {
                            existingShopId = pmReader.IsDBNull(0) ? 0 : pmReader.GetInt32(0);
                            existingRecId = pmReader.IsDBNull(1) ? 0 : pmReader.GetInt64(1);
                        }
                        pmReader.Close();
                        if (existingShopId > 0)
                        {
                            // Delete dependent Project row(s)
                            var delProj = connection.CreateCommand();
                            delProj.CommandText = @"DELETE FROM Project WHERE ShopTicketID = $stid";
                            delProj.Parameters.AddWithValue("$stid", existingShopId);
                            delProj.ExecuteNonQuery();

                            // Delete the old ShopTicket
                            var delTicket = connection.CreateCommand();
                            delTicket.CommandText = @"DELETE FROM ShopTicket WHERE ShopTicketID = $stid";
                            delTicket.Parameters.AddWithValue("$stid", existingShopId);
                            delTicket.ExecuteNonQuery();

                            // Remove orphaned Rectangle if not referenced by any ShopTicket
                            if (existingRecId > 0)
                            {
                                var rectStillUsed = connection.CreateCommand();
                                rectStillUsed.CommandText = @"SELECT 1 FROM ShopTicket WHERE RecID = $rec LIMIT 1";
                                rectStillUsed.Parameters.AddWithValue("$rec", existingRecId);
                                var used = rectStillUsed.ExecuteScalar();
                                if (used == null)
                                {
                                    var delRect = connection.CreateCommand();
                                    delRect.CommandText = @"DELETE FROM Rectangle WHERE RecID = $rec";
                                    delRect.Parameters.AddWithValue("$rec", existingRecId);
                                    delRect.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Prevent duplicate ShopTicket rows by FileName (if same exact file uploaded again, prefer replacing)
                    var dupCheck = connection.CreateCommand();
                    dupCheck.CommandText = @"SELECT ShopTicketID, RecID FROM ShopTicket WHERE FileName = $fn LIMIT 1;";
                    dupCheck.Parameters.AddWithValue("$fn", pdf.FileName);
                    using (var r = dupCheck.ExecuteReader())
                    {
                        int existingShopId = 0; long existingRecId = 0;
                        if (r.Read())
                        {
                            existingShopId = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            existingRecId = r.IsDBNull(1) ? 0 : r.GetInt64(1);
                        }
                        r.Close();
                        if (existingShopId > 0)
                        {
                            var delProj = connection.CreateCommand();
                            delProj.CommandText = @"DELETE FROM Project WHERE ShopTicketID = $stid";
                            delProj.Parameters.AddWithValue("$stid", existingShopId);
                            delProj.ExecuteNonQuery();

                            var delTicket = connection.CreateCommand();
                            delTicket.CommandText = @"DELETE FROM ShopTicket WHERE ShopTicketID = $stid";
                            delTicket.Parameters.AddWithValue("$stid", existingShopId);
                            delTicket.ExecuteNonQuery();

                            if (existingRecId > 0)
                            {
                                var rectStillUsed = connection.CreateCommand();
                                rectStillUsed.CommandText = @"SELECT 1 FROM ShopTicket WHERE RecID = $rec LIMIT 1";
                                rectStillUsed.Parameters.AddWithValue("$rec", existingRecId);
                                var used = rectStillUsed.ExecuteScalar();
                                if (used == null)
                                {
                                    var delRect = connection.CreateCommand();
                                    delRect.CommandText = @"DELETE FROM Rectangle WHERE RecID = $rec";
                                    delRect.Parameters.AddWithValue("$rec", existingRecId);
                                    delRect.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    long newRecID;

                    // First, try to find an existing matching rectangle
                    var commandSelectRect = connection.CreateCommand();
                    commandSelectRect.CommandText =
                    @"
                    SELECT RecID FROM Rectangle
                    WHERE RectanglePage = $page
                      AND FormViewRectangleX = $fvrx
                      AND FormViewRectangleY = $fvry
                      AND FormViewRectangleWidth = $fvrw
                      AND FormViewRectangleHeight = $fvrh;
                    ";
                    commandSelectRect.Parameters.AddWithValue("$page", pdf.RectanglePage);
                    commandSelectRect.Parameters.AddWithValue("$fvrx", pdf.FormViewRectangleX);
                    commandSelectRect.Parameters.AddWithValue("$fvry", pdf.FormViewRectangleY);
                    commandSelectRect.Parameters.AddWithValue("$fvrw", pdf.FormViewRectangleWidth);
                    commandSelectRect.Parameters.AddWithValue("$fvrh", pdf.FormViewRectangleHeight);
                   
                    // No matching rectangle found, insert a new one
                    var commandInsertRect = connection.CreateCommand();
                    commandInsertRect.CommandText =
                    @"
                    INSERT INTO Rectangle (
                        RectanglePage, FormViewRectangleX, FormViewRectangleY, FormViewRectangleWidth, FormViewRectangleHeight, SectoinViewRectangleX, SectionViewRectangleY, SectioniewRectangleWidth, SectionViewRectangleHeight       
                    ) VALUES ($page, $fvrx, $fvry, $fvrw, $fvrh, $svrx, $svry, $svrw, $svrh);
                    SELECT last_insert_rowid();
                    ";
                    // Reuse the parameters defined above
                    commandInsertRect.Parameters.AddWithValue("$page", pdf.RectanglePage);
                    commandInsertRect.Parameters.AddWithValue("$fvrx", pdf.FormViewRectangleX);
                    commandInsertRect.Parameters.AddWithValue("$fvry", pdf.FormViewRectangleY);
                    commandInsertRect.Parameters.AddWithValue("$fvrw", pdf.FormViewRectangleWidth);
                    commandInsertRect.Parameters.AddWithValue("$fvrh", pdf.FormViewRectangleHeight);
                    commandInsertRect.Parameters.AddWithValue("$svrx", 0);
                    commandInsertRect.Parameters.AddWithValue("$svry", 0);
                    commandInsertRect.Parameters.AddWithValue("$svrw", 0);
                    commandInsertRect.Parameters.AddWithValue("$svrh", 0);

                    var insRectRes = commandInsertRect.ExecuteScalar();
                    if (insRectRes == null) throw new InvalidOperationException("Failed to retrieve new RecID.");
                    newRecID = Convert.ToInt64(insRectRes);

                    // 2. Insert data into the Child table (ShopTicket) using newRecID (FK)
                    var commandShop = connection.CreateCommand();
                    commandShop.CommandText =
                    @"
                    INSERT INTO ShopTicket (
                        ProjectName, ProjectNumber, DesignNumber, PiecesRequired, ControlNumbers,PageNames, 
                        Weight, FileContentPieceMark, FileName, FileNamePieceMark, NumberOfPages, PdfBlob, RecID
                    ) VALUES ($pn, $prnu, $dn, $pire, $cn, $pnames, $we, $fcpm, $fn, $fnpm, $nop, $blob, $recid);
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
                    commandShop.Parameters.AddWithValue("$blob", (object?)pdf.PdfBytes);
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
                        s.Weight, s.DesignNumber, s.PdfBlob, r.RectanglePage,
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
                byte[] pdfBlob = pdfBlob = reader.IsDBNull(11) ? Array.Empty<byte>() : (byte[])reader.GetValue(11);
                int rectanglePage = reader.IsDBNull(12) ? 0 : reader.GetInt32(12);
                double rectX = reader.IsDBNull(13) ? 0 : reader.GetDouble(13);
                double rectY = reader.IsDBNull(14) ? 0 : reader.GetDouble(14);
                double rectW = reader.IsDBNull(15) ? 0 : reader.GetDouble(15);
                double rectH = reader.IsDBNull(16) ? 0 : reader.GetDouble(16);
                string dateCreatedRaw = reader.IsDBNull(17) ? string.Empty : reader.GetString(17);

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

                ShopTicket ticket = new ShopTicket(
                    loggerFactory,
                    pdfBlob,
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
                    rectanglePage,
                    rectX,
                    rectY,
                    rectW,
                    rectH,
                    null,
                    null,
                    null,
                    null,
                    processed
                );

                result.Add(ticket);
            }
            return result;
        }

        public void RemoveRowByFileName(string fileName)
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

                        // 1. Find the ShopTicketID by FileName
                        using (var commandFindShopTicketId = connection.CreateCommand())
                        {
                            commandFindShopTicketId.CommandText = "SELECT ShopTicketID FROM ShopTicket WHERE FileName = $fileName";
                            commandFindShopTicketId.Parameters.AddWithValue("$fileName", fileName);
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
                                Console.WriteLine($"FileName '{fileName}' not found. Cannot proceed with deletion.");
                                transaction.Rollback();
                                return;
                            }
                        }

                        // 2. Delete the Project record(s) linked to this ShopTicketID
                        using (var commandDeleteProject = connection.CreateCommand())
                        {
                            commandDeleteProject.CommandText = "DELETE FROM Project WHERE ShopTicketID = $shopTicketID";
                            commandDeleteProject.Parameters.AddWithValue("$shopTicketID", shopTicketID);
                            commandDeleteProject.ExecuteNonQuery();
                            Console.WriteLine($"Deleted Project records linked to ShopTicketID {shopTicketID}.");
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
                        }

                        // 4. Delete the ShopTicket record itself
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

                        // 5. Delete the related Rectangle record if RecID exists
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

                        // Commit transaction
                        transaction.Commit();
                        Console.WriteLine($"Successfully removed all associated data starting from FileName '{fileName}'.");
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

       
        public int DeduplicateByPieceMark(bool keepLatest = false)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Create a temp table listing duplicate ShopTicketIDs to delete
                using (var createTemp = connection.CreateCommand())
                {
                    createTemp.Transaction = transaction;
                    createTemp.CommandText = @"
                        CREATE TEMP TABLE dup_tickets AS
                        SELECT ShopTicketID FROM (
                            SELECT ShopTicketID, FileNamePieceMark,
                                   ROW_NUMBER() OVER (
                                       PARTITION BY FileNamePieceMark
                                       ORDER BY " + (keepLatest ? "ShopTicketID DESC" : "ShopTicketID ASC") + @"
                                   ) AS rn
                            FROM ShopTicket
                            WHERE FileNamePieceMark IS NOT NULL AND FileNamePieceMark <> ''
                        ) WHERE rn > 1;
                    ";
                    createTemp.ExecuteNonQuery();
                }

                // Delete dependent Project rows referencing duplicate tickets
                using (var delProjects = connection.CreateCommand())
                {
                    delProjects.Transaction = transaction;
                    delProjects.CommandText = @"
                        DELETE FROM Project
                        WHERE ShopTicketID IN (SELECT ShopTicketID FROM dup_tickets);
                    ";
                    delProjects.ExecuteNonQuery();
                }

                // Count duplicates prior to deletion
                int dupCount = 0;
                using (var countDup = connection.CreateCommand())
                {
                    countDup.Transaction = transaction;
                    countDup.CommandText = @"SELECT COUNT(*) FROM dup_tickets";
                    var o = countDup.ExecuteScalar();
                    dupCount = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
                }

                // Delete duplicate ShopTicket rows
                using (var delTickets = connection.CreateCommand())
                {
                    delTickets.Transaction = transaction;
                    delTickets.CommandText = @"
                        DELETE FROM ShopTicket
                        WHERE ShopTicketID IN (SELECT ShopTicketID FROM dup_tickets);
                    ";
                    delTickets.ExecuteNonQuery();
                }

                // Remove orphan Rectangle rows (no longer referenced by any ShopTicket)
                using (var delRects = connection.CreateCommand())
                {
                    delRects.Transaction = transaction;
                    delRects.CommandText = @"
                        DELETE FROM Rectangle
                        WHERE RecID NOT IN (
                            SELECT RecID FROM ShopTicket WHERE RecID IS NOT NULL
                        );
                    ";
                    delRects.ExecuteNonQuery();
                }

                // Drop temp table
                using (var dropTemp = connection.CreateCommand())
                {
                    dropTemp.Transaction = transaction;
                    dropTemp.CommandText = @"DROP TABLE dup_tickets";
                    dropTemp.ExecuteNonQuery();
                }

                transaction.Commit();
                return dupCount;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
