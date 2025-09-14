using SimpleDispatch.ServiceBase;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Database;
using simpledispatch_unitservice.Repositories;

namespace simpledispatch_unitservice;

public class UnitService : BaseService
{
    public UnitService(string[] args) : base(args)
    {
    }

    protected override void RegisterMessageHandler()
    {
        Builder.Services.AddScoped<IMessageHandler, UnitMessageHandler>();
    }

    protected override void ConfigureDatabase()
    {
        ConfigurePostgreSqlDbContext<BaseDbContext>();
    }

    protected override void ConfigureServices()
    {
        Builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
    }
}
