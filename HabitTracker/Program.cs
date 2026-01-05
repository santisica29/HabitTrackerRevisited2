using Microsoft.Data.Sqlite;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HabitTracker
{
    class Program
    {
        static string connectionString = @"Data Source=habitTracker.db";

        static void Main(string[] args)
        {

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS coding (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT,
                        Quantity INTEGER
                        )";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }

            GetUserInput();
        }

        static void GetUserInput()
        {
            Console.Clear();
            bool closeApp = false;
            while (closeApp == false)
            {
                Console.WriteLine("\n\nMAIN MENU");
                Console.WriteLine("\nType 0 to Close the Application");
                Console.WriteLine("Type 1 to View All Records");
                Console.WriteLine("Type 2 to Insert Record");
                Console.WriteLine("Type 3 to Delete Record");
                Console.WriteLine("Type 4 to Update Record");
                LineBreak();

                string commandInput = Console.ReadLine();

                switch (commandInput)
                {
                    case "0":
                        Console.WriteLine("\nGoodbye");
                        closeApp = true;
                        break;
                    case "1":
                        GetAllRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Delete();
                        break;
                    case "4":
                        Update();
                        break;
                    default:
                        Console.WriteLine("Invalid command. Type from 0 to 4");
                        break;
                }

            }
        }

        private static void GetAllRecords()
        {
            Console.Clear();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"SELECT * FROM coding";

                List<CodingRecord> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                while (reader.Read())
                {
                    tableData.Add(
                        new CodingRecord
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", CultureInfo.InvariantCulture),
                            Quantity = reader.GetInt32(2)
                        });
                }

                connection.Close();

                LineBreak();

                if (tableData.Count == 0)
                {
                    Console.WriteLine("No rows found");
                }
                else
                {
                    foreach (var cr in tableData)
                    {
                        Console.WriteLine($"{cr.Id} - {cr.Date.ToString("dd MMMM yy")} - Quantity: {cr.Quantity}");
                    }
                }

                LineBreak();
            }

        }

        private static void Insert()
        {
            string date = GetDateInput();

            int quantity = GetNumberInput("\nInsert number of minutes of coding time (no decimals)");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"INSERT INTO coding(date, quantity) VALUES ('{date}', {quantity})";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        private static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\nType the Id of the record you want to delete. Press 0 to go back to the Main Menu.");

            if (recordId == 0) GetUserInput();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"DELETE from coding WHERE Id = {recordId}";
                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"Record with the Id: {recordId} doesn't exist.");
                    Console.ReadKey();
                    Delete();
                    return;
                }

                Console.WriteLine("Record deleted successfully!");

                connection.Close();
            }
        }

        internal static void Update()
        {
            Console.Clear();
            GetAllRecords();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var recordId = GetNumberInput("Type Id of the record to update.");

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM coding WHERE ID = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"Record with Id {recordId} doesn't exist.\n");
                    Console.ReadKey();
                    connection.Close();
                    Update();
                    return;
                }

                string date = GetDateInput();
                int quantity = GetNumberInput("Select new quantity for the coding record");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE coding SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

                tableCmd.ExecuteNonQuery();

                Console.WriteLine("Record updated!");

                connection.Close();
            }
        }
        internal static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            int finalInput = Convert.ToInt32(numberInput);

            return finalInput;
        }

        internal static string GetDateInput()
        {
            Console.WriteLine("\nPLease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.");

            string dateInput = Console.ReadLine();

            if (dateInput == "0") GetUserInput();

            while (!DateTime.TryParseExact(dateInput, "dd-mm-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                Console.WriteLine("Invalid date: (Format dd-mm-yy)");
                dateInput = Console.ReadLine();
            }

            return dateInput;
        }

        internal static void LineBreak()
        {
            Console.WriteLine("---------------------------------\n");
        }
    }

    public class CodingRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}



