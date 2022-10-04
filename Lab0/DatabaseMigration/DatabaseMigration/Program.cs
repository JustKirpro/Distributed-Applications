using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using DatabaseMigration.Data_structures;
using DatabaseMigration.Databases;
using Newtonsoft.Json;
using Npgsql;

namespace DatabaseMigration
{
    public static class Program
    {
        private static void Main()
        {
            try
            {
                var records = SqliteDatabase.ReadRecords();
                PostgresqlDatabase.CreateDatabaseSchema();
                PostgresqlDatabase.FillDatabase(records);
                
                var jsonPath = ConfigurationManager.AppSettings.Get("JSONPath");
                var data = PostgresqlDatabase.ReadPlaylistSongTable();
                SerializeRecords(jsonPath, data);
                
                var scriptPath = ConfigurationManager.AppSettings.Get("PythonScriptPath");
                var pythonPath = ConfigurationManager.AppSettings.Get("PythonPath");
                var excelPath = ConfigurationManager.AppSettings.Get("ExcelPath");
                var arguments = new List<string>
                {
                    jsonPath,
                    excelPath
                };
                
                var pythonExecutor = new PythonExecutor(pythonPath, scriptPath, arguments);
                pythonExecutor.RunScript();
            }
            catch (Exception exception) when (exception is SQLiteException or NpgsqlException or IOException)
            {
                 Console.WriteLine($"An exception has occured: {exception.Message}");
            }
        }

        private static void SerializeRecords(string path, List<Table> data)
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        }
    }
}