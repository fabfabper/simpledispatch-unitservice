using Microsoft.EntityFrameworkCore;
using SimpleDispatch.SharedModels.Entities;
using SimpleDispatch.ServiceBase.Database;

namespace simpledispatch_unitservice.Data;

public class UnitDbContext : BaseDbContext
{
    public UnitDbContext(DbContextOptions<UnitDbContext> options) : base(options)
    {
    }

    public DbSet<Unit> Units { get; set; } = null!;

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        // The Unit entity configuration is handled by the SharedModels package
        // No additional configuration needed
    }
}
