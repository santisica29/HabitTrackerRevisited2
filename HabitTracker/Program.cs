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

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(
                            new CodingRecord
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                                Quantity = reader.GetInt32(2)
                            });
                    }
                }
                else
                {
                    Console.WriteLine("No rows found");
                }

                connection.Close();

                LineBreak();

                foreach (var cr in tableData)
                {
                    Console.WriteLine($"{cr.Id} - {cr.Date.ToString("dd MMMM yy")} - Quantity: {cr.Quantity}");
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
                    $"INSERT INTO coding(date, quantity) VALUES ('{date}', '{quantity}')";

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
                tableCmd.CommandText = $"DELETE from coding WHERE Id = '{recordId}'";
                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"Record with the Id: '{recordId}' doesn't exist.");
                    Console.ReadKey();
                    Delete();
                } 
                    
                Console.WriteLine("Record deleted successfully!");

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



