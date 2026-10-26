using System.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ElevatorControl.Data
{
    // Everything about CONNECTING to the database and CREATING it lives here.
    // Nothing in this class knows about elevators.
    public static class DatabaseHelper
    {
        // The connection string is read from App.config, not written in the code
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["ElevatorDb"];
                if (settings == null)
                {
                    throw new ConfigurationErrorsException("The 'ElevatorDb' connection string is missing from App.config.");
                }
                return settings.ConnectionString;
            }
        }

        // Same server, but connected to "master" (needed to CREATE the database)
        private static string MasterConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
                builder.InitialCatalog = "master";
                return builder.ConnectionString;
            }
        }

        // Creates the database and tables if they are not there yet.
        public static void EnsureDatabase()
        {
            RunScript(MasterConnectionString, "01_CreateDatabase.sql");
            RunScript(ConnectionString, "02_CreateTables.sql");
        }

        // RELATIVE PATH: the scripts are found from the folder the program runs in,
        // so the project works on any computer (no "C:\Users\..." anywhere).
        private static string GetScriptPath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Database", fileName);
        }

        private static void RunScript(string connectionString, string fileName)
        {
            string path = GetScriptPath(fileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("SQL script not found: " + path);
            }

            string script = File.ReadAllText(path);

            // SQL Server cannot run the word GO from C#, so we split the script on it
            string[] batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (string batch in batches)
                    {
                        if (string.IsNullOrWhiteSpace(batch))
                        {
                            continue;
                        }

                        // ExecuteNonQuery is fine here: this only runs the CREATE scripts.
                        // The log rows themselves are saved with a DataAdapter (see ElevatorLogRepository).
                        using (SqlCommand command = new SqlCommand(batch, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(
                    "Could not run " + fileName + ". Is SQL Server (SQLEXPRESS) running? " + ex.Message, ex);
            }
        }
    }
}
