using System;
using System.Configuration;
using System.Net.Sockets;
using Npgsql;
using RabbitMQ.Client.Exceptions;

namespace Normalization.Server;

public static class Program
{
    public static void Main()
    {
        var useRabbit = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("UseRabbit"));

        if (useRabbit)
        {
            StartRabbitServer();
        }
        else
        {
            StartSocketServer();
        }
    }

    private static void StartSocketServer()
    {
        try
        {
            var server = new SocketServer();
            server.Start();
        }
        catch (NpgsqlException)
        {
            Console.WriteLine("An error occured while creating database or writing to the database.");
        }
        catch (Exception exception) when (exception is SocketException or InvalidOperationException or ObjectDisposedException)
        {
            Console.WriteLine("An error occured while receiving records from clients.");
        }
    }
    
    private static void StartRabbitServer()
    {
        try
        {
            var server = new RabbitServer();
            server.Start();
        }
        catch (NpgsqlException)
        {
            Console.WriteLine("An error occured while creating database or writing to the database.");
        }
        catch (Exception exception) when (exception is BrokerUnreachableException or OperationInterruptedException)
        {
            Console.WriteLine("An error occured while consuming the messages from the queue");
        }
    }
}