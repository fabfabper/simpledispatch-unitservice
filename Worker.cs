using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using simpledispatch_unitservice.Repositories;

namespace simpledispatch_unitservice;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMQConfiguration _rabbitMQConfig;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IConnection? _connection;
    private IChannel? _channel;

    public Worker(
        ILogger<Worker> logger, 
        IOptions<RabbitMQConfiguration> rabbitMQConfig,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _rabbitMQConfig = rabbitMQConfig.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeRabbitMQ();
        await base.StartAsync(cancellationToken);
    }

    private async Task InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitMQConfig.HostName,
                Port = _rabbitMQConfig.Port,
                UserName = _rabbitMQConfig.UserName,
                Password = _rabbitMQConfig.Password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare the queue (create if it doesn't exist)
            await _channel.QueueDeclareAsync(
                queue: _rabbitMQConfig.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("Connected to RabbitMQ and declared queue: {QueueName}", _rabbitMQConfig.QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("RabbitMQ channel is not initialized");
            return;
        }

        try
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation("Received message from queue '{QueueName}': {Message}", 
                        _rabbitMQConfig.QueueName, message);

                    // Example: Get units from database when a message is received
                    await LogDatabaseUnits();

                    // Acknowledge the message
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    // Reject the message and requeue it
                    await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _rabbitMQConfig.QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Started consuming messages from queue: {QueueName}", _rabbitMQConfig.QueueName);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteAsync");
        }
    }

    private async Task LogDatabaseUnits()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var databaseRepository = scope.ServiceProvider.GetRequiredService<IDatabaseRepository>();
            
            var units = await databaseRepository.GetAllUnitsAsync();
            _logger.LogInformation("Found {UnitCount} units in database", units.Count());

            foreach (var unit in units.Take(5)) // Log first 5 units
            {
                _logger.LogInformation("Unit: ID={UnitId}, Status={UnitStatus}", 
                    unit.Id, unit.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving units from database");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
