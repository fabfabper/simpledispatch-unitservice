using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.SharedModels.Entities;
using simpledispatch_unitservice.Repositories;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace simpledispatch_unitservice;

public class UnitMessageHandler : IMessageHandler
{
    private readonly ILogger<UnitMessageHandler> _logger;
    private readonly IDatabaseRepository _databaseRepository;

    public UnitMessageHandler(
        ILogger<UnitMessageHandler> logger,
        IDatabaseRepository databaseRepository)
    {
        _logger = logger;
        _databaseRepository = databaseRepository;
    }

    public async Task HandleMessageAsync(string message, BasicDeliverEventArgs args)
    {
        try
        {
            _logger.LogInformation("Processing message: {Message}", message);

            // Try to parse the message as JSON to extract message type and unit information
            var messageData = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
            
            if (messageData?.ContainsKey("messageType") == true)
            {
                var messageType = messageData["messageType"].ToString();
                _logger.LogInformation("Processing message of type: {MessageType}", messageType);

                // Process the message based on its type
                switch (messageType)
                {
                    case "UnitCreated":
                    case "UnitUpdated":
                    case "UnitStatusChanged":
                        await ProcessUnitUpdate();
                        break;
                    case "UnitDeleted":
                        await ProcessUnitDeletion();
                        break;
                    default:
                        _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                        break;
                }
            }
            else
            {
                // Fallback: just log all units for any message
                await LogDatabaseUnits();
            }

            _logger.LogInformation("Successfully processed message");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse message as JSON, treating as plain text");
            await LogDatabaseUnits();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            throw;
        }
    }

    private async Task ProcessUnitUpdate()
    {
        // Log all units when receiving a unit update message
        await LogDatabaseUnits();
    }

    private async Task ProcessUnitDeletion()
    {
        _logger.LogInformation("Processing unit deletion");
        await LogDatabaseUnits();
    }

    private async Task LogDatabaseUnits()
    {
        try
        {
            var units = await _databaseRepository.GetAllAsync();
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
}
