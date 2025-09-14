using Microsoft.EntityFrameworkCore;
using SimpleDispatch.SharedModels.Entities;
using SimpleDispatch.ServiceBase.Database;

namespace simpledispatch_unitservice.Repositories;

public class DatabaseRepository : BaseRepository<Unit, string, BaseDbContext>, IDatabaseRepository
{
    public DatabaseRepository(BaseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Unit>> GetUnitsByStatusAsync(int status)
    {
        try
        {
            return await FindAsync(u => u.Status == status);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving units with status: {status}", ex);
        }
    }

    public async Task<IEnumerable<Unit>> GetActiveUnitsAsync()
    {
        try
        {
            // Assuming active units have a specific status (e.g., status != 0 or status == 1)
            // You can adjust this logic based on your business rules
            return await FindAsync(u => u.Status > 0);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving active units", ex);
        }
    }
}
