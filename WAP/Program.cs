using Newtonsoft.Json;
using System;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WAP
{
   
    class Program
    {
        static string apiKey = "b2577b0ebf54bd0bac7c255747a06981";
        static async Task Main(string[] args)
        {
            Console.Write("Введіть місто:");
            string city = Console.ReadLine();

            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            CreateTable(sqlite_conn);

            while (true)
            {
                string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";
                Console.WriteLine("Якщо ви хочете отримати актуальну інформацію натисніть: 1");
                Console.WriteLine("Якщо ви хочете переглянути результати попередніх запитів натисніть: 2");
                Console.WriteLine($"Якщо ви хочете обрати інше місто, а не {city} натисніть: 3");
                Console.WriteLine($"Зараз обране місто:{city}");
                var choice = Console.ReadKey();
                switch (choice.KeyChar)
                {
                    case '1':
                        Console.WriteLine();
                        await DataEntry(apiUrl, sqlite_conn);
                        ReadLastData(sqlite_conn);
                        break;

                    case '2':
                        Console.WriteLine();
                        ReadAllData(sqlite_conn);
                        break;

                    case '3':
                        
                        Console.Write("Введіть місто:");
                        city=Console.ReadLine();
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Невірний вибір.");
                        break;
                }
            }
        }


        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return sqlite_conn;
        }


        static void CreateTable(SQLiteConnection conn)
        {

            SQLiteCommand sqlite_cmd;
            string createTableQuery = @"
               CREATE TABLE IF NOT EXISTS Weather (
                   Id INTEGER PRIMARY KEY AUTOINCREMENT,
                   Name TEXT,
                   Weather_Main TEXT,
                   Weather_Description TEXT,
                   Temp REAL,
                   Pressure INTEGER,
                   Humidity INTEGER,
                   WindSpeed REAL,
                   WindDeg INTEGER,
                   WindGust REAL,
                   Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
               );";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = createTableQuery;
            sqlite_cmd.ExecuteNonQuery();
        }

        static async Task DataEntry(string apiUrl, SQLiteConnection conn)
        {
            using (HttpClient httpClient = new HttpClient())
            {  
             HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

              if (response.IsSuccessStatusCode)
              {
                string responseData = await response.Content.ReadAsStringAsync();
                WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(responseData);
                    EntryWeatherData(conn, weatherData);
              }
              else
              {
                Console.WriteLine($"Помилка отримання даних з API: {response.StatusCode}");
              }
                
            }
        }

        static void EntryWeatherData(SQLiteConnection conn, WeatherData weatherData)
        {
            string insertDataQuery = @"
            INSERT INTO Weather (
            Name, Weather_Main, Weather_Description, Temp, 
            Pressure, Humidity, WindSpeed, WindDeg, WindGust) 
            VALUES (
            @Name, @Weather_Main, @Weather_Description, @Temp,
            @Pressure, @Humidity, @WindSpeed, @WindDeg, @WindGust);";
            using (SQLiteCommand command = new SQLiteCommand(insertDataQuery, conn))
            {
            command.Parameters.AddWithValue("@Name", weatherData.Name);
            command.Parameters.AddWithValue("@Weather_Main", weatherData.Weather?.FirstOrDefault()?.Main);
            command.Parameters.AddWithValue("@Weather_Description", weatherData.Weather?.FirstOrDefault()?.Description);
            command.Parameters.AddWithValue("@Temp", weatherData.Main.Temp);
            command.Parameters.AddWithValue("@Pressure", weatherData.Main.Pressure);
            command.Parameters.AddWithValue("@Humidity", weatherData.Main.Humidity);
            command.Parameters.AddWithValue("@WindSpeed", weatherData.Wind.Speed);
            command.Parameters.AddWithValue("@WindDeg", weatherData.Wind.Deg);
            command.Parameters.AddWithValue("@WindGust", weatherData.Wind.Gust);
            command.ExecuteNonQuery();
            }
    
        }

        static void ReadAllData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Weather";
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            while (sqlite_datareader.Read())
            {
                for (int i = 0; i < sqlite_datareader.FieldCount; i++)
                {
                    string columnName = sqlite_datareader.GetName(i);
                    string columnValue = sqlite_datareader.GetValue(i).ToString();
                    Console.WriteLine($"{columnName}: {columnValue}");
                }
                Console.WriteLine();
            }
        }

        static void ReadLastData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Weather ORDER BY ID DESC LIMIT 1";
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            while (sqlite_datareader.Read())
            {
                for (int i = 0; i < sqlite_datareader.FieldCount; i++)
                {
                    string columnName = sqlite_datareader.GetName(i);
                    string columnValue = sqlite_datareader.GetValue(i).ToString();
                    Console.WriteLine($"{columnName}: {columnValue}");
                }
                Console.WriteLine();
            }
        }
    }
}


       

