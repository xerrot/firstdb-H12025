using System;
using Npgsql;
using System.Collections.Generic;

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

        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "…";
        }

        public void PrintAllTableDetails()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            // Hent alle tabeller i public schema
            var getTablesCmd = new NpgsqlCommand(
                "SELECT table_name FROM information_schema.tables WHERE table_schema='public' ORDER BY table_name", conn);
            using var tablesReader = getTablesCmd.ExecuteReader();

            var tables = new List<string>();
            while (tablesReader.Read())
            {
                tables.Add(tablesReader.GetString(0));
            }
            tablesReader.Close();

            foreach (var table in tables)
            {
                // Hent primærnøgler for tabellen
                var pkCmd = new NpgsqlCommand(@"
                    SELECT kcu.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu
                      ON tc.constraint_name = kcu.constraint_name
                     AND tc.table_schema = kcu.table_schema
                    WHERE tc.constraint_type = 'PRIMARY KEY'
                      AND tc.table_schema = 'public'
                      AND tc.table_name = @table", conn);
                pkCmd.Parameters.AddWithValue("table", table);

                var primaryKeys = new HashSet<string>();
                using (var pkReader = pkCmd.ExecuteReader())
                {
                    while (pkReader.Read())
                    {
                        primaryKeys.Add(pkReader.GetString(0));
                    }
                }

                // Hent fremmednøgler for tabellen
                var fkCmd = new NpgsqlCommand(@"
                    SELECT kcu.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu
                      ON tc.constraint_name = kcu.constraint_name
                     AND tc.table_schema = kcu.table_schema
                    WHERE tc.constraint_type = 'FOREIGN KEY'
                      AND tc.table_schema = 'public'
                      AND tc.table_name = @table", conn);
                fkCmd.Parameters.AddWithValue("table", table);

                var foreignKeys = new HashSet<string>();
                using (var fkReader = fkCmd.ExecuteReader())
                {
                    while (fkReader.Read())
                    {
                        foreignKeys.Add(fkReader.GetString(0));
                    }
                }

                Console.WriteLine($"\nTabel: {table}");
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("{0,-22} {1,-13} {2,-12} {3,-6} {4,-6}", "Kolonne", "Datatype", "Nullable", "PK", "FK");
                Console.WriteLine("-----------------------------------------------------------------------");

                var getColumnsCmd = new NpgsqlCommand(
                    @"SELECT column_name, data_type, is_nullable
                      FROM information_schema.columns
                      WHERE table_schema = 'public' AND table_name = @table", conn);
                getColumnsCmd.Parameters.AddWithValue("table", table);

                using var columnsReader = getColumnsCmd.ExecuteReader();
                while (columnsReader.Read())
                {
                    string colName = Truncate(columnsReader.GetString(0), 22);
                    string dataType = Truncate(columnsReader.GetString(1), 13);
                    string isNullable = columnsReader.GetString(2);
                    string isPrimary = primaryKeys.Contains(columnsReader.GetString(0)) ? "Yes" : "";
                    string isForeign = foreignKeys.Contains(columnsReader.GetString(0)) ? "Yes" : "";
                    Console.WriteLine("{0,-22} {1,-13} {2,-12} {3,-6} {4,-6}", colName, dataType, isNullable, isPrimary, isForeign);
                }
                columnsReader.Close();
            }
        }
    }
}
