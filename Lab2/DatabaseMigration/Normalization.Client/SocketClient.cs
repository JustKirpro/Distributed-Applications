using System;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Normalization.Client.Database;

namespace Normalization.Client;

internal class SocketClient
{
    private readonly string _hostName;
    private readonly int _port;

    public SocketClient()
    {
        _hostName = ConfigurationManager.AppSettings.Get("Hostname");
        _port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SocketPort"));
    }
    
    /// <summary>
    /// <exception cref="SQLiteException"> SQLite database problem has occured. </exception>
    /// <exception cref="SocketException"> An error occurred when accessing the socket. </exception>
    /// <exception cref="ObjectDisposedException"> The <see cref="TcpClient"/> is not connected to a remote host. </exception>
    /// </summary>
    public void Start()
    {
        var records = SqliteDatabase.ReadRecords();

        using var client = new TcpClient();
        client.Connect(_hostName, _port);
        
        Console.WriteLine("Connected to the server.");

        using var stream = client.GetStream();
        
        var recordNumber = 1;
        foreach (var record in records)
        {
            var serializedObject = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(record));
            var length = BitConverter.GetBytes(serializedObject.Length);
            var data = length.Concat(serializedObject).ToArray();
            stream.Write(data, 0, data.Length);
            Console.WriteLine($"Record #{recordNumber++} sent.");
        }
        
        client.Client.Shutdown(SocketShutdown.Send);
        
        Console.WriteLine("Closed the connection to the server.");
    }
}