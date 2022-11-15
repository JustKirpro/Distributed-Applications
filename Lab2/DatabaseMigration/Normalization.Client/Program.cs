using System;
using System.Configuration;
using System.Data.SQLite;
using System.Net.Sockets;
using RabbitMQ.Client.Exceptions;

namespace Normalization.Client;

public static class Program
{
    public static void Main()
    {
        var useRabbit = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("UseRabbit"));

        if (useRabbit)
        {
            StartRabbitClient();
        }
        else
        {
            StartSocketClient();
        }
    }

    private static void StartSocketClient()
    {
        try
        {
            var client = new SocketClient();
            client.Start();
        }
        catch (SQLiteException)
        {
            Console.WriteLine("An error occured while reading the records from the database.");
        }
        catch (Exception exception) when (exception is SocketException or ObjectDisposedException)
        {
            Console.WriteLine("An error occured while connected or sending the data to the server.");
        }
    }

    private static void StartRabbitClient()
    {
        try
        {
            var client = new RabbitClient();
            client.Start();
        }
        catch (SQLiteException)
        {
            Console.WriteLine("An error occured while reading the records from the database.");
        }
        catch (BrokerUnreachableException)
        {
            Console.WriteLine("An error occured while publishing the messages to the queue");
        }
    }
}