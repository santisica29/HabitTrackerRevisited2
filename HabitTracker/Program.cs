using Microsoft.Data.Sqlite;
using System.Collections.Generic;
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
                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS habits (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Unit TEXT NOT NULL
                        )";

                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS habitsLog (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                        HabitId INTEGER NOT NULL,
                        FOREIGN KEY (HabitId) 
                            REFERENCES habits(Id) 
                            ON DELETE CASCADE
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
                Console.WriteLine("Type 1 to View All Logs");
                Console.WriteLine("Type 2 to Insert Record");
                Console.WriteLine("Type 3 to Delete Record");
                Console.WriteLine("Type 4 to Update Record");
                Console.WriteLine("Type 5 to Create a Habit");
                Console.WriteLine("Type 6 to Delete a Habit");

                LineBreak();

                string commandInput = Console.ReadLine();

                switch (commandInput)
                {
                    case "0":
                        Console.WriteLine("\nGoodbye");
                        closeApp = true;
                        Environment.Exit(0);
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
                    case "5":
                        CreateHabit();
                        break;
                    case "6":
                        DeleteHabit();
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
                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText =
                    @"SELECT 
                            habits.Id,
                            habitsLog.Id,
                            habits.Name,
                            habitsLog.Date,
                            habits.Unit,
                            habitsLog.Quantity
                    FROM habitsLog
                    JOIN habits ON habitsLog.HabitId = habits.Id";

                List<HabitWithLogDTO> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                while (reader.Read())
                {
                    tableData.Add(
                        new HabitWithLogDTO
                        {
                            HabitId = reader.GetInt32(0),
                            HabitLogId = reader.GetInt32(1),
                            HabitName = reader.GetString(2),
                            Date = DateTime.ParseExact(reader.GetString(3), "dd-MM-yy", CultureInfo.InvariantCulture),
                            Unit = reader.GetString(4),
                            Quantity = reader.GetInt32(5),
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
                    foreach (var habit in tableData)
                    {
                        Console.WriteLine($"{habit.HabitLogId} - {habit.Date.ToString("dd MMMM yy")} - Name: {habit.HabitName} - Unit:{habit.Unit} - Quantity: {habit.Quantity}");
                    }
                }

                LineBreak();
            }
        }

        private static List<Habit> GetAllHabits()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "SELECT * FROM habits";

                var list = new List<Habit>();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Habit
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = Convert.ToString(reader["Name"]),
                        Unit = reader["Unit"].ToString(),
                    });
                }

                connection.Close();

                LineBreak();

                if (list.Count == 0)
                {
                    Console.WriteLine("No habits found");
                }
                else
                {
                    foreach (var h in list)
                    {
                        Console.WriteLine($"{h.Id} - Name: {h.Name} - Unit:{h.Unit}");
                    }
                }

                LineBreak();
                connection.Close();

                return list;
            }
        }

        private static void Insert()
        {
            var listOfHabits = GetAllHabits();
            int habitId;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                Console.WriteLine("Choose your habit name:");
                var habitName = Console.ReadLine().Trim().ToLower();

                var checkCmd = connection.CreateCommand();

                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM habits WHERE Name = @Name)";
                checkCmd.Parameters.AddWithValue("@Name", habitName);

                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"Habit with Name {habitName} doesn't exist.\n");
                    Console.WriteLine("Create it? (y/n)");
                    string userInput = Console.ReadLine().Trim().ToLower();
                    if (userInput == "y")
                    {
                        habitId = CreateHabit(habitName);
                    }
                    else
                    {
                        GetUserInput();
                        return;
                    }
                }
                else
                {
                    var getIdCmd = connection.CreateCommand();
                    getIdCmd.CommandText = "SELECT habits.Id FROM habits WHERE habits.Name = @Name";
                    getIdCmd.Parameters.AddWithValue("@Name", habitName);

                    habitId = (int)getIdCmd.ExecuteScalar();
                }

                string date = GetDateInput();

                int quantity = GetNumberInput("\nInsert quantity: ");

                tableCmd.CommandText =
                    $"INSERT INTO habitsLog(date, quantity, habitId) VALUES (@Date, @Quantity, @HabitId)";

                tableCmd.Parameters.AddWithValue("@Date", date);
                tableCmd.Parameters.AddWithValue("@Quantity", quantity);
                tableCmd.Parameters.AddWithValue("@HabitId", habitId);

                tableCmd.ExecuteNonQuery();
            }
        }

        private static int CreateHabit(string name = null)
        {
            if (name == null)
            {
                Console.WriteLine("\nChoose your habit's name:");
                name = Console.ReadLine();
            }

            Console.WriteLine("\nChoose your habit's unit of measurements:");
            string unit = Console.ReadLine();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "INSERT INTO habits(name, unit) VALUES (@Name, @Unit)";

                tableCmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
                tableCmd.Parameters.Add("@Unit", SqliteType.Text).Value = unit;

                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "SELECT last_insert_rowid()";
                long lastId = (long)tableCmd.ExecuteScalar();

                return (int)lastId;
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

                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = $"DELETE from habitsLog WHERE Id = @Id";
                tableCmd.Parameters.Add("@Id", SqliteType.Integer).Value = recordId;

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"Record with the Id: {recordId} doesn't exist.");
                    Console.ReadKey();
                    Delete();
                    return;
                }

                Console.WriteLine("Record deleted successfully!");
                Console.ReadLine();

                connection.Close();
            }
        }

        private static void DeleteHabit()
        {
            Console.Clear();
            GetAllHabits();

            var habitId = GetNumberInput("\nType the Id of the habit you want to delete. Press 0 to go back to the Main Menu.");

            if (habitId == 0) GetUserInput();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = $"DELETE from habits WHERE Id = @Id";
                tableCmd.Parameters.AddWithValue("@Id", habitId);

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"Record with the Id: {habitId} doesn't exist.");
                    Console.ReadKey();
                    Delete();
                    return;
                }

                Console.WriteLine("Record deleted successfully!");
                Console.ReadLine();

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
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM coding WHERE ID = @Id)";
                checkCmd.Parameters.AddWithValue("@Id", recordId);

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
                int quantity = GetNumberInput("Select new quantity for the habit log");

                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = "PRAGMA foreign_keys = ON";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = $"UPDATE habitsLog SET date = @Date, quantity = @Quantity WHERE Id = @Id";
                tableCmd.Parameters.Add("@Date", SqliteType.Text).Value = date;
                tableCmd.Parameters.Add("@Quantity", SqliteType.Integer).Value = quantity;
                tableCmd.Parameters.Add("@Id", SqliteType.Integer).Value = recordId;

                tableCmd.ExecuteNonQuery();

                Console.WriteLine("Record updated!");

                connection.Close();
            }
        }
        internal static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
            {
                Console.WriteLine("\nInvalid number. Try again.");
                numberInput = Console.ReadLine();
            }

            int finalInput = Convert.ToInt32(numberInput);

            return finalInput;
        }

        internal static string GetDateInput()
        {
            Console.WriteLine("\nPLease insert the date: (Format: dd-mm-yy). Type 't' to insert today's date. Type 0 to return to main menu.");

            string dateInput = Console.ReadLine();

            if (dateInput == "0") GetUserInput();

            if (dateInput.ToLower() == "t")
            {
                DateTime date = DateTime.Today;
                dateInput = date.ToString("dd-MM-yy");
            }

            while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
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

    public class Habit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
    }

    public class HabitLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public Habit Habit { get; set; }
    }

    public class HabitWithLogDTO
    {
        public int HabitId { get; init; }
        public int HabitLogId { get; init; }
        public string HabitName { get; init; }
        public DateTime Date { get; init; }
        public string Unit { get; init; }
        public int Quantity { get; init; }
    }
}



