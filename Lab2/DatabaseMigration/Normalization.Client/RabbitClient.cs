using System;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using Normalization.Client.Database;
using RabbitMQ.Client;

namespace Normalization.Client;

internal class RabbitClient
{
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _queueName;
    
    public RabbitClient()
    {
        _hostName = ConfigurationManager.AppSettings.Get("Hostname");
        _port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("RabbitPort"));
        _userName = ConfigurationManager.AppSettings.Get("RabbitUsername");
        _password = ConfigurationManager.AppSettings.Get("RabbitPassword");
        _queueName = ConfigurationManager.AppSettings.Get("RabbitQueueName");
    }

    public void Start()
    {
        var records = SqliteDatabase.ReadRecords();
        
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

        var recordNumber = 1;
        foreach (var record in records)
        {
            var json = JsonConvert.SerializeObject(record);
            PublishMessage(channel, json);
            Console.WriteLine($"Record #{recordNumber++} published.");
        }
        
        Console.WriteLine("All the records have been published.");
    }

    private void PublishMessage(IModel channel, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
    }
}