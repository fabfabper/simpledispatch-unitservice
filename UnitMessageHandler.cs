using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.SharedModels.Commands;
using simpledispatch_unitservice.Repositories;
using RabbitMQ.Client.Events;
using SimpleDispatch.SharedModels.CommandTypes;

namespace simpledispatch_unitservice;

public class UnitMessageHandler : IMessageHandler
{
    private readonly ILogger<UnitMessageHandler> _logger;
    private readonly IDatabaseRepository _databaseRepository;
    private readonly IRabbitMqProducer _producer;

    public UnitMessageHandler(
        ILogger<UnitMessageHandler> logger,
        IDatabaseRepository databaseRepository,
        IRabbitMqProducer producer)
    {
        _logger = logger;
        _databaseRepository = databaseRepository;
        _producer = producer;
    }

    public async Task HandleMessageAsync(string message, BasicDeliverEventArgs args)
    {
        try
        {
            _logger.LogInformation("Processing message: {Message}", message);

            var unitCommand = System.Text.Json.JsonSerializer.Deserialize<UnitCommand>(message);
            if (unitCommand == null)
            {
                _logger.LogWarning("Failed to deserialize message as UnitCommand");
                await LogDatabaseUnits();
                return;
            }

            var unitEntity = UnitCommandConverter.ToUnit(unitCommand);

            _logger.LogInformation("Received UnitCommand: Command={CommandType}, UnitId={UnitId}", unitCommand.Command, unitCommand.Id);

            switch (unitCommand.Command)
            {
                case UnitCommandType.UpdateUnit:
                case UnitCommandType.UpdateUnitStatus:
                    await ProcessUnitUpdate(unitEntity);
                    break;
                // case UnitCommandType.:
                //     await ProcessUnitDeletion();
                //     break;
                default:
                    _logger.LogWarning("Unknown Command: {Command}", unitCommand.Command);
                    break;
            }

            // Publish the UnitCommand to RabbitMQ after processing
            var serialized = System.Text.Json.JsonSerializer.Serialize(unitCommand);
            await _producer.PublishAsync(serialized, "notifications");
            _logger.LogInformation("Published UnitCommand to RabbitMQ after processing");

            _logger.LogInformation("Successfully processed UnitCommand");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse message as UnitCommand JSON");
            await LogDatabaseUnits();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UnitCommand message");
            throw;
        }
    }

    private async Task ProcessUnitUpdate(SimpleDispatch.SharedModels.Entities.Unit unit)
    {
        await _databaseRepository.UpdateAsync(unit);
        await _databaseRepository.SaveChangesAsync();
        _logger.LogInformation("Unit updated in database: {UnitId}", unit.Id);
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
