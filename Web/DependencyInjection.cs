using Repositories.Base;
using Services;
using Microsoft.EntityFrameworkCore;
using Contract.Services.Interface;
using Services.Service;
using Net.payOS;
using Microsoft.Extensions.Caching.Memory;
using System;

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
            services.AddPayment(configuration);
            services.ConfigureServices();

        }
        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }
        public static void AddPayment(this IServiceCollection services, IConfiguration configuration)
        {
            PayOS payOS = new PayOS(configuration["PayOS:PAYOS_CLIENT_ID"] ?? throw new Exception("Cannot find environment"),
                    configuration["PayOS:PAYOS_API_KEY"] ?? throw new Exception("Cannot find environment"),
                    configuration["PayOS:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Cannot find environment"));
            services.AddSingleton(payOS);
        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ComesticsSalesDBContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("DBConnection"), b =>
        b.MigrationsAssembly("Repositories"));
            });
            //services.AddDbContext<ComesticsSalesDBContext>(options =>
            //options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        }
        public static void AddServices(this IServiceCollection services)
        {
            services
                .AddScoped<GeminiService>()
                .AddScoped<ISupplierService, SupplierService>()
                .AddScoped<IChatService, ChatService>()
                .AddScoped<ICartService, CartService>()
                .AddScoped<IOrderService, OrderService>()
                .AddScoped<IUserService, UserService>()
                .AddTransient<IEmailSender, EmailSender>()
                .AddScoped<TokenService>()
                .AddScoped<ICateogoryService, CateogoryService>();
        }
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddHostedService<PaymentProcessingService>();
            services.AddMemoryCache();
        }
    }
}
