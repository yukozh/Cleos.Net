using Microsoft.Extensions.DependencyInjection;

namespace Andoromeda.CleosNet.Agent.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAgentServices(this IServiceCollection self)
        {
            return self.AddSingleton<OneBoxService>()
                .AddSingleton<ProcessService>();
        }
    }
}
