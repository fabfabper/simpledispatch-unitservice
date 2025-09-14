using simpledispatch_unitservice;

namespace simpledispatch_unitservice;

public class Program
{
    public static async Task Main(string[] args)
    {
        var service = new UnitService(args);
        await service.RunAsync();
    }
}
