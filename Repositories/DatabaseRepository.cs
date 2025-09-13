using Microsoft.EntityFrameworkCore;
using SimpleDispatch.SharedModels.Entities;
using simpledispatch_unitservice.Data;

namespace simpledispatch_unitservice.Repositories;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseRepository> _logger;

    public DatabaseRepository(ApplicationDbContext context, ILogger<DatabaseRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Unit>> GetAllUnitsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all units from database");
            return await _context.Units.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all units from database");
            throw;
        }
    }

    public async Task<Unit?> GetUnitByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving unit with ID: {UnitId}", id);
            return await _context.Units.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unit with ID: {UnitId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Unit>> GetUnitsByStatusAsync(int status)
    {
        try
        {
            _logger.LogInformation("Retrieving units with status: {Status}", status);
            return await _context.Units
                .Where(u => u.Status == status)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving units with status: {Status}", status);
            throw;
        }
    }

    public async Task<Unit> CreateUnitAsync(Unit unit)
    {
        try
        {
            _logger.LogInformation("Creating new unit: {UnitId}", unit.Id);
            
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully created unit with ID: {UnitId}", unit.Id);
            return unit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit: {UnitId}", unit.Id);
            throw;
        }
    }

    public async Task<Unit?> UpdateUnitAsync(Unit unit)
    {
        try
        {
            _logger.LogInformation("Updating unit with ID: {UnitId}", unit.Id);
            
            var existingUnit = await _context.Units.FindAsync(unit.Id);
            if (existingUnit == null)
            {
                _logger.LogWarning("Unit with ID: {UnitId} not found for update", unit.Id);
                return null;
            }

            existingUnit.Id = unit.Id;
            existingUnit.Type = unit.Type;
            existingUnit.Status = unit.Status;
            existingUnit.Latitude = unit.Latitude;
            existingUnit.Longitude = unit.Longitude;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully updated unit with ID: {UnitId}", unit.Id);
            return existingUnit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit with ID: {UnitId}", unit.Id);
            throw;
        }
    }

    public async Task<bool> DeleteUnitAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting unit with ID: {UnitId}", id);
            
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                _logger.LogWarning("Unit with ID: {UnitId} not found for deletion", id);
                return false;
            }

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully deleted unit with ID: {UnitId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit with ID: {UnitId}", id);
            throw;
        }
    }

    public async Task<bool> UnitExistsAsync(string id)
    {
        try
        {
            return await _context.Units.AnyAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if unit exists with ID: {UnitId}", id);
            throw;
        }
    }

    public Task<IEnumerable<Unit>> GetActiveUnitsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> UnitExistsAsync(int id)
    {
        throw new NotImplementedException();
    }
}
