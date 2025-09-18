using SimpleDispatch.ServiceBase;
using SimpleDispatch.ServiceBase.Interfaces;
using simpledispatch_unitservice.Data;
using simpledispatch_unitservice.Repositories;
using SimpleDispatch.ServiceBase.Extensions;

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
        ConfigurePostgreSqlDbContext<UnitDbContext>();
    }

    protected override void ConfigureServices()
    {
    Builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
    Builder.Services.AddRabbitMqProducer(Builder.Configuration);
    }
}
