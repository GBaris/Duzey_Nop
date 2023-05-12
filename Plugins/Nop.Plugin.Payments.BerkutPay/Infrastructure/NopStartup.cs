using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.BerkutPay.Services.IServices;

namespace Nop.Plugin.Payments.BerkutPay.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public int Order => 1;

        public void Configure(IApplicationBuilder application)
        {
            
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IYKB_Service, IYKB_Service>();
        }
    }
}
