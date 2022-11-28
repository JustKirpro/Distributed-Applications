using System;
using System.Configuration;
using Grpc.Core;
using Normalization.Client.Database;

namespace Normalization.Client;

public static class Program
{
    public static void Main()
    {
        var hostName = ConfigurationManager.AppSettings.Get("Hostname");
        var port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Port"));
        
        var records = SqliteDatabase.ReadRecords();

        var channel = new Channel($"{hostName}:{port}", ChannelCredentials.Insecure);
        var client = new Server.Normalization.NormalizationClient(channel);

        try
        {
            for (var i = 0; i < records.Count; i++)
            {
                var reply = client.Normalize(records[i]);
                Console.WriteLine(reply.IsSuccessful ? $"Record #{i + 1} successfully inserted" : "An insertion error has occured");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Connection error has occured");
        }
    }
}