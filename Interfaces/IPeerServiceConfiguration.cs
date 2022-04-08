using Microsoft.Extensions.DependencyInjection;

namespace CoreLibrary.Interfaces;

public interface IPeerServiceConfiguration
{
    IServiceCollection ConfigureServices(IServiceCollection services);
}
