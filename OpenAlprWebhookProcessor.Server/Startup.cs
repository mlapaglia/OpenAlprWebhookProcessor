using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
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

            services.AddAutoMapperConfiguration();

            services.AddMemoryCache();

            services.AddDevelopmentDataSeeding();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OpenALPR Webhook Processor API",
                    Version = "v1",
                    Description = "API for managing license plate recognition, cameras, alerts, and machine learning configurations"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenALPR Webhook Processor API v1");
                    c.RoutePrefix = "swagger";
                });
            }

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
