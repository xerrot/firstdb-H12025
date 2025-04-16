using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Blazor.Services
{
    public class DBService
    {
        private readonly string _connectionString;

        public DBService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetAllTableNamesAsync()
        {
            var tables = new List<string>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var command = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema='public'", conn);
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
            return tables;
        }

        public async Task<string> GetMermaidSchemaAsync()
        {
            var tables = new List<string>();
            var relations = new List<(string FromTable, string FromCol, string ToTable, string ToCol)>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Hent tabeller
            var tableCmd = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema='public'", conn);
            var tableReader = await tableCmd.ExecuteReaderAsync();
            while (await tableReader.ReadAsync())
                tables.Add(tableReader.GetString(0));
            await tableReader.DisposeAsync();

            // Hent relationer
            var relCmd = new NpgsqlCommand(@"
                SELECT
                    tc.table_name AS from_table,
                    kcu.column_name AS from_column,
                    ccu.table_name AS to_table,
                    ccu.column_name AS to_column
                FROM
                    information_schema.table_constraints AS tc
                    JOIN information_schema.key_column_usage AS kcu
                      ON tc.constraint_name = kcu.constraint_name
                      AND tc.table_schema = kcu.table_schema
                    JOIN information_schema.constraint_column_usage AS ccu
                      ON ccu.constraint_name = tc.constraint_name
                      AND ccu.table_schema = tc.table_schema
                WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_schema = 'public';
            ", conn);
            var relReader = await relCmd.ExecuteReaderAsync();
            while (await relReader.ReadAsync())
                relations.Add((
                    relReader.GetString(0),
                    relReader.GetString(1),
                    relReader.GetString(2),
                    relReader.GetString(3)
                ));
            await relReader.DisposeAsync();

            // Byg Mermaid-diagram
            var mermaid = "erDiagram\n";
            foreach (var table in tables)
                mermaid += $"    {table} {{}}\n";
            foreach (var rel in relations)
                mermaid += $"    {rel.FromTable} }}o--|| {rel.ToTable} : \"{rel.FromCol} → {rel.ToCol}\"\n";
            return mermaid;
        }
    }
}