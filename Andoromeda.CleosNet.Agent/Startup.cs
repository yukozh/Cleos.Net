using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Andoromeda.CleosNet.Agent.Services;

namespace Andoromeda.CleosNet.Agent
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAgentServices();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseErrorHandlingMiddleware();
            app.UseMvcWithDefaultRoute();
        }
    }
}
