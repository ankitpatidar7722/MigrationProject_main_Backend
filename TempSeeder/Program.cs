using System;
using System.IO;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=DESKTOP-533GP0U;Database=MigraTrackDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
        string scriptPath = Path.Combine("..", "CreateUsersTable.sql");

        try
        {
            if (!File.Exists(scriptPath))
            {
                Console.WriteLine($"Error: Script not found at {Path.GetFullPath(scriptPath)}");
                return;
            }

            string script = File.ReadAllText(scriptPath);
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database.");

                // Split script by GO if exists (simple split), or just run executing if it's single batch.
                // The current script provided doesn't use GO, it uses IF/BEGIN/END which is one batch valid.
                
                using (SqlCommand command = new SqlCommand(script, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Script executed successfully.");
                    Console.WriteLine("Table 'Users' checked/created and default admin user ensured.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
