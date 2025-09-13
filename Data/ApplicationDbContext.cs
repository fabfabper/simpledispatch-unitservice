using Microsoft.EntityFrameworkCore;
using SimpleDispatch.SharedModels.Entities;

namespace simpledispatch_unitservice.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Unit> Units { get; set; }
}
