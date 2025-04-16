using System;
using Npgsql;

namespace KonsolApp.Services
{
    public class DBService
    {
        private readonly string _connectionString;

        public DBService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void PrintAllTableNames()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var command = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema='public'", conn);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine(reader["table_name"]);
            }
        }
    }
}
