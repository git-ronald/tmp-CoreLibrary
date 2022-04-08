using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Interfaces;

public interface IPeerStartup
{
    Task MigrateDatabase(AsyncServiceScope scope);
}
