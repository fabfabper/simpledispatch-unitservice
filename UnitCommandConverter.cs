using SimpleDispatch.SharedModels.Commands;
using SimpleDispatch.SharedModels.Entities;

namespace simpledispatch_unitservice;

public static class UnitCommandConverter
{
    public static Unit ToUnit(UnitCommand command)
    {
        return new Unit
        {
            Id = command.Payload.Id,
            Type = command.Payload.Type,
            Status = command.Payload.Status,
            Latitude = command.Payload.Position?.Latitude,
            Longitude = command.Payload.Position?.Longitude,
            
        };
    }
}
