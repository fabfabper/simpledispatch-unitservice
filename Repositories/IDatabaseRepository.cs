using SimpleDispatch.SharedModels.Entities;
using SimpleDispatch.ServiceBase.Database.Interfaces;

namespace simpledispatch_unitservice.Repositories;

public interface IDatabaseRepository : IRepository<Unit, string>
{
    Task<IEnumerable<Unit>> GetUnitsByStatusAsync(int status);
    Task<IEnumerable<Unit>> GetActiveUnitsAsync();
}
