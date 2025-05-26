using Repositories.Base;
using Services;
using Microsoft.EntityFrameworkCore;
using Contract.Services.Interface;
using Services.Service;

namespace Web
{
    public static class DependencyInjection
    {
        public static void AddConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigRoute();
            services.AddDatabase(configuration);
            //services.AddIdentity();
            services.AddInfrastructure(configuration);
            services.AddServices();
        }
        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ComesticsSalesDBContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("DBConnection"), b =>
        b.MigrationsAssembly("Repositories"));
            });
        }
        public static void AddServices(this IServiceCollection services)
        {
            services
                .AddScoped<ICateogoryService, CateogoryService>();
        }
    }
}
