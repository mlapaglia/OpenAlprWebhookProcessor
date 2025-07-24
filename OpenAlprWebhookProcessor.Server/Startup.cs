using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using OpenAlprWebhookProcessor.Infrastructure.Extensions;
using Serilog;
using System;

namespace OpenAlprWebhookProcessor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddHttpClient();
            services.AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache());
            services.AddApplicationServices(Configuration);

            services.AddDataServices(Configuration);

            services.AddJwtAuthentication(Configuration);

            services.AddScoped<IUserService, UserService>();
            
            services.AddScoped<IUsersUnitOfWork, UsersUnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJwtKeyRepository, JwtKeyRepository>();

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordService, PasswordService>();

            services.AddExternalServices();

            services.AddBackgroundServices();

            services.AddMachineLearningServices();

            services.AddAutoMapper();

            services.AddMemoryCache();

            services.AddDevelopmentDataSeeding();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseExceptionHandling();

            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };

            app.UseWebSockets(webSocketOptions);

            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/index.html");
                endpoints.MapHub<ProcessorHub.ProcessorHub>("/api/processorHub");
            });
        }
    }
}
