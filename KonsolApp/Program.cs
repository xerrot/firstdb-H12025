using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using KonsolApp.Services;

namespace KonsolApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Indlæs appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Indlæs connection string fra appsettings.json
            var connectionString = config.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string not found in appsettings.json");

            // Opret en instans af DBService
            var dbService = new DBService(connectionString);

            // Tjek forbindelse til databasen
            if (dbService.TestConnection())
            {
                Console.WriteLine("Forbindelse til databasen lykkedes!");
                // Print alle tabeller i databasen
                dbService.PrintAllTableNames();
            }
            else
            {
                Console.WriteLine("Kunne ikke oprette forbindelse til databasen.");
            }
        }
    }
}
