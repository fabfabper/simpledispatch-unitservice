using Microsoft.EntityFrameworkCore;
using SimpleDispatch.SharedModels.Entities;
using SimpleDispatch.ServiceBase.Database;
using simpledispatch_unitservice.Data;

namespace simpledispatch_unitservice.Repositories;

public class DatabaseRepository : BaseRepository<Unit, string, UnitDbContext>, IDatabaseRepository
{
    public DatabaseRepository(UnitDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Unit>> GetUnitsByStatusAsync(int status)
    {
        return await FindAsync(u => u.Status == status);
    }

    public async Task<IEnumerable<Unit>> GetActiveUnitsAsync()
    {
        // Assuming active units have a specific status (e.g., status > 0)
        return await FindAsync(u => u.Status > 0);
    }
}
