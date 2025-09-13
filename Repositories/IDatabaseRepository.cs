using SimpleDispatch.SharedModels.Entities;

namespace simpledispatch_unitservice.Repositories;

public interface IDatabaseRepository
{
    Task<IEnumerable<Unit>> GetAllUnitsAsync();
    Task<Unit?> GetUnitByIdAsync(int id);
    Task<IEnumerable<Unit>> GetUnitsByStatusAsync(int status);
    Task<Unit> CreateUnitAsync(Unit unit);
    Task<Unit?> UpdateUnitAsync(Unit unit);
    Task<bool> DeleteUnitAsync(int id);
}
