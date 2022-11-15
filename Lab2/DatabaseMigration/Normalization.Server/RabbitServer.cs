using System;
using System.Configuration;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Normalization.Server.Database;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Normalization.Server;

public class RabbitServer
{
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _queueName;
    
    /// <summary>
    /// <exception cref="NpgsqlException"> PostgreSQL database problem has occured. </exception>
    /// <exception cref="IOException"> SQL script file was not found. </exception>
    /// </summary>
    public RabbitServer()
    {
        _hostName = ConfigurationManager.AppSettings.Get("Hostname");
        _port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("RabbitPort"));
        _userName = ConfigurationManager.AppSettings.Get("RabbitUsername");
        _password = ConfigurationManager.AppSettings.Get("RabbitPassword");
        _queueName = ConfigurationManager.AppSettings.Get("RabbitQueueName");
        PostgresqlDatabase.CreateDatabaseSchema();
    }

    /// <summary>
    /// <exception cref="NpgsqlException"> PostgreSQL database problem has occured. </exception>
    /// <exception cref="BrokerUnreachableException"> An error occurred while consuming the messages from the queue. </exception>
    /// </summary>
    public void Start()
    {
        Console.WriteLine("Launching rabbit server...");

        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName,
            Password = _password
        };
        
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: true, arguments: null);
        
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var record =  JsonConvert.DeserializeObject<Record>(message);
            PostgresqlDatabase.InsertRecord(record);
            Console.WriteLine("Record received");
        };

        channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        
        Console.ReadKey();
        Console.WriteLine("Stopping rabbit server");
    }
}