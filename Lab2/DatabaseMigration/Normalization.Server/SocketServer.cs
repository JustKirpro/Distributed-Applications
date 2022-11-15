using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Normalization.Server.Database;
using Npgsql;

namespace Normalization.Server;

public class SocketServer
{
    private readonly IPAddress _ipAddress;
    private readonly int _port;

    /// <summary>
    /// <exception cref="NpgsqlException"> PostgreSQL database problem has occured. </exception>
    /// <exception cref="IOException"> SQL script file was not found. </exception>
    /// </summary>
    public SocketServer()
    {
        _ipAddress = IPAddress.Parse(ConfigurationManager.AppSettings.Get("Hostname"));
        _port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SocketPort"));
        PostgresqlDatabase.CreateDatabaseSchema();
    }
    
    /// <summary>
    /// <exception cref="NpgsqlException"> PostgreSQL database problem has occured. </exception>
    /// <exception cref="SocketException"> An error occurred when accessing the socket. </exception>
    /// </summary>
    public void Start()
    {
        var server = new TcpListener(_ipAddress, _port);
        server.Start();
        
        Console.WriteLine($"Launching socket server at {server.LocalEndpoint}...");
        
        while (true)
        {
            Console.WriteLine("Waiting for new connections... ");
                    
            using var client = server.AcceptTcpClient();
            
            Console.WriteLine($"Client connection from {client.Client.RemoteEndPoint} accepted.");

            using var stream = client.GetStream();
            var lengthArray = new byte[4];

            var recordNumber = 1;
            do
            {
                stream.Read(lengthArray, 0, lengthArray.Length);
                var length = BitConverter.ToInt32(lengthArray);
                var data = new byte[length];
                stream.Read(data, 0, data.Length);
                var json = Encoding.UTF8.GetString(data, 0, length);

                var record = DeserializeRecord(json);
                PostgresqlDatabase.InsertRecord(record);
                Console.WriteLine($"Record #{recordNumber++} received");
            } while (stream.DataAvailable);
            
            Console.WriteLine($"Closing connection with client at {client.Client.RemoteEndPoint}.");
        }
    }

    private static Record DeserializeRecord(string json) => JsonConvert.DeserializeObject<Record>(json);
}