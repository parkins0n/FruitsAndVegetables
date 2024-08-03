using System;
using System.Data.SqlClient;

class Program
{
    static string server = "parkinson\\STEP";
    static string serverConnectionString = $"Server={server};Trusted_Connection=True;";
    static string databaseConnectionString = $"Server={server};Database=FruitsAndVegetables;Trusted_Connection=True;";

    static void Main(string[] args)
    {
        CreateDatabase();

        using (SqlConnection connection = new SqlConnection(databaseConnectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("Успішне підключення до бази даних 'Овочі та фрукти'.");

                CreateTableIfNotExists(connection);
                InsertTestData(connection);

                DisplayAllItems(connection);
                DisplayAllNames(connection);
                DisplayAllColors(connection);
                DisplayMaxCalories(connection);
                DisplayMinCalories(connection);
                DisplayAvgCalories(connection);
                DisplayVegetableCount(connection);
                DisplayFruitCount(connection);
                DisplayCountByColor(connection, "red");
                DisplayCountByEachColor(connection);
                DisplayItemsBelowCalories(connection, 50);
                DisplayItemsAboveCalories(connection, 100);
                DisplayItemsInCalorieRange(connection, 50, 100);
                DisplayYellowOrRedItems(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка підключення до бази даних: {ex.Message}");
            }
        }
    }
    static void CreateDatabase()
    {
        using (SqlConnection connection = new SqlConnection(serverConnectionString))
        {
            try
            {
                connection.Open();
                string createDbQuery = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FruitsAndVegetables') CREATE DATABASE FruitsAndVegetables";
                using (SqlCommand command = new SqlCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                Console.WriteLine("Базу даних 'FruitsAndVegetables' створено або вона вже існує.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка створення бази даних: {ex.Message}");
            }
        }
    }

    static void CreateTableIfNotExists(SqlConnection connection)
    {
        string createTableQuery = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
            BEGIN
                CREATE TABLE Items (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(50),
                    Type NVARCHAR(50),
                    Color NVARCHAR(50),
                    Calories INT
                );
            END";

        using (SqlCommand command = new SqlCommand(createTableQuery, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    static void InsertTestData(SqlConnection connection)
    {
        string[] items = new string[]
        {
        "('Apple', 'фрукт', 'red', 52)",
        "('Banana', 'фрукт', 'yellow', 89)",
        "('Carrot', 'овоч', 'orange', 41)",
        "('Lettuce', 'овоч', 'green', 15)",
        "('Strawberry', 'фрукт', 'red', 32)",
        "('Tomato', 'овоч', 'red', 18)",
        "('Blueberry', 'фрукт', 'blue', 57)",
        "('Pumpkin', 'овоч', 'orange', 26)",
        "('Cucumber', 'овоч', 'green', 16)",
        "('Grapefruit', 'фрукт', 'yellow', 42)"
        };

        foreach (var item in items)
        {
            string[] parts = item.Trim('(', ')').Split(',');

            string name = parts[0].Trim('\'', ' ');
            string type = parts[1].Trim('\'', ' ');
            string color = parts[2].Trim('\'', ' ');
            string calories = parts[3].Trim(' ');

            string checkQuery = @"
            IF NOT EXISTS (SELECT 1 FROM Items WHERE Name = @Name AND Type = @Type AND Color = @Color)
            BEGIN
                INSERT INTO Items (Name, Type, Color, Calories) VALUES (@Name, @Type, @Color, @Calories)
            END";

            using (SqlCommand command = new SqlCommand(checkQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Type", type);
                command.Parameters.AddWithValue("@Color", color);
                command.Parameters.AddWithValue("@Calories", calories);
                command.ExecuteNonQuery();
            }
        }
    }

    static void DisplayAllItems(SqlConnection connection)
    {
        Console.WriteLine("Всі елементи:");
        string query = "SELECT * FROM Items";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Name"]}, {reader["Type"]}, {reader["Color"]}, {reader["Calories"]}");
            }
            reader.Close();
        }
        Console.WriteLine();
    }

    static void DisplayAllNames(SqlConnection connection)
    {
        Console.WriteLine("Усі назви:");
        string query = "SELECT Name FROM Items";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(reader["Name"]);
            }
            reader.Close();
        }
        Console.WriteLine();
    }

    static void DisplayAllColors(SqlConnection connection)
    {
        Console.WriteLine("Усі кольори:");
        string query = "SELECT DISTINCT Color FROM Items";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(reader["Color"]);
            }
            reader.Close();
        }
        Console.WriteLine();
    }

    static void DisplayMaxCalories(SqlConnection connection)
    {
        Console.WriteLine("Максимальна калорійність:");
        string query = "SELECT MAX(Calories) FROM Items";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            var result = command.ExecuteScalar();
            Console.WriteLine(result);
        }
        Console.WriteLine();
    }

    static void DisplayMinCalories(SqlConnection connection)
    {
        Console.WriteLine("Мінімальна калорійність:");
        string query = "SELECT MIN(Calories) FROM Items";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            var result = command.ExecuteScalar();
            Console.WriteLine(result);
        }
        Console.WriteLine();
    }

    static void DisplayAvgCalories(SqlConnection connection)
    {
        Console.WriteLine("Середня калорійність:");
        string query = "SELECT AVG(Calories) FROM Items";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            var result = command.ExecuteScalar();
            Console.WriteLine(result);
        }
        Console.WriteLine();
    }

    static void DisplayVegetableCount(SqlConnection connection)
    {
        Console.WriteLine("Кількість овочів:");
        string query = "SELECT COUNT(*) FROM Items WHERE Type='овоч'";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            var result = command.ExecuteScalar();
            Console.WriteLine(result);
        }
        Console.WriteLine();
    }

    static void DisplayFruitCount(SqlConnection connection)
    {
        Console.WriteLine("Кількість фруктів:");
        string query = "SELECT COUNT(*) FROM Items WHERE Type='фрукт'";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            var result = command.ExecuteScalar();
            Console.WriteLine(result);
        }
        Console.WriteLine();
    }

    static void DisplayCountByColor(SqlConnection connection, string color)
    {
        Console.WriteLine($"Кількість овочів і фруктів кольору {color}:");
        string query = "SELECT COUNT(*) FROM Items WHERE Color=@color";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@color", color);
            var result = command.ExecuteScalar();
            Console.WriteLine(result);
        }
        Console.WriteLine();
    }

    static void DisplayCountByEachColor(SqlConnection connection)
    {
        Console.WriteLine("Кількість овочів і фруктів кожного кольору:");
        string query = "SELECT Color, COUNT(*) FROM Items GROUP BY Color";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Color"]}: {reader[1]}");
            }
            reader.Close();
        }
        Console.WriteLine();
    }

    static void DisplayItemsBelowCalories(SqlConnection connection, int calories)
    {
        Console.WriteLine($"Елементи з калорійністю нижче {calories}:");
        string query = "SELECT * FROM Items WHERE Calories < @calories";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@calories", calories);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Name"]}, {reader["Type"]}, {reader["Color"]}, {reader["Calories"]}");
            }
            reader.Close();
        }
        Console.WriteLine();
    }

    static void DisplayItemsAboveCalories(SqlConnection connection, int calories)
    {
        Console.WriteLine($"Елементи з калорійністю вище {calories}:");
        string query = "SELECT * FROM Items WHERE Calories > @calories";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@calories", calories);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Name"]}, {reader["Type"]}, {reader["Color"]}, {reader["Calories"]}");
            }
            reader.Close();
        }
        Console.WriteLine();
    }

    static void DisplayItemsInCalorieRange(SqlConnection connection, int minCalories, int maxCalories)
    {
        Console.WriteLine($"Елементи з калорійністю від {minCalories} до {maxCalories}:");
        string query = "SELECT * FROM Items WHERE Calories BETWEEN @minCalories AND @maxCalories";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@minCalories", minCalories);
            command.Parameters.AddWithValue("@maxCalories", maxCalories);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Name"]}, {reader["Type"]}, {reader["Color"]}, {reader["Calories"]}");
            }
            reader.Close();
        }
        Console.WriteLine();
    }

    static void DisplayYellowOrRedItems(SqlConnection connection)
    {
        Console.WriteLine("Овочі та фрукти жовтого або червоного кольору:");
        string query = "SELECT * FROM Items WHERE Color='yellow' OR Color='red'";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Name"]}, {reader["Type"]}, {reader["Color"]}, {reader["Calories"]}");
            }
            reader.Close();
        }
        Console.WriteLine();
    }
}