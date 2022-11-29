using System;
using System.Configuration;
using System.Threading.Tasks;
using Grpc.Core;
using Normalization.Client.Database;

namespace Normalization.Client;

public static class Program
{
    public static async Task Main()
    {
        var hostName = ConfigurationManager.AppSettings.Get("HostName");
        var port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Port"));
        
        var records = SqliteDatabase.ReadRecords();

        var channel = new Channel($"{hostName}:{port}", ChannelCredentials.Insecure);
        var client = new Server.Normalization.NormalizationClient(channel);

        try
        {
            var stream=client.Normalize();
            
            foreach (var record in records)
            {  
                await stream.RequestStream.WriteAsync(record);
            }

            await stream.RequestStream.CompleteAsync();
            await stream.ResponseAsync;
            Console.WriteLine("Records successfully inserted");
        }
        catch (Exception)
        {
            Console.WriteLine("Connection error has occured");
        }
    }
}