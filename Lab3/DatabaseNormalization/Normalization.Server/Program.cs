using System;
using Grpc.Core;
using System.Configuration;
using Normalization.Server.Database;

namespace Normalization.Server;

public static class Program
{
    public static void Main()
    {
        PostgresqlDatabase.CreateDatabaseSchema();
        
        var hostName = ConfigurationManager.AppSettings.Get("Hostname");
        var port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Port"));

        var server = new Grpc.Core.Server
        {
            Services = {Normalization.BindService(new NormalizationService())},
            Ports = {new ServerPort(hostName, port, ServerCredentials.Insecure)}
        };
        
        server.Start();

        Console.WriteLine("Server is launched.\nIn order to stop server press any button.");
        Console.ReadKey();
    }
}