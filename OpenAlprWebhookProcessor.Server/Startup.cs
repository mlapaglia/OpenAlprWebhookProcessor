using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenAlprWebhookProcessor.Features.Users.Services;
using OpenAlprWebhookProcessor.Infrastructure.Extensions;
using OpenAlprWebhookProcessor.Infrastructure.Middleware;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            services.AddApplicationServices();

            services.AddDataServices(Configuration);

            services.AddIdentity<Features.Users.Data.ApplicationUser, IdentityRole<int>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;

                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            })
            .AddEntityFrameworkStores<Features.Users.Data.UsersContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/account/access-denied";
                
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddScoped<IPasswordService, PasswordService>();

            // Configure FIDO2/WebAuthn
            services.AddFido2(options =>
            {
                var serverDomain = Environment.GetEnvironmentVariable("FIDO2_SERVER_DOMAIN");
                var originsEnv = Environment.GetEnvironmentVariable("FIDO2_ORIGINS");
                
                // Default to localhost for development if not configured
                if (string.IsNullOrEmpty(serverDomain))
                {
                    serverDomain = "localhost";
                    Log.Warning("FIDO2_SERVER_DOMAIN environment variable not set, defaulting to 'localhost'. This should be set for production deployments.");
                }
                
                string[] origins;
                if (string.IsNullOrEmpty(originsEnv))
                {
                    origins = new[] { "https://localhost:4200", "https://localhost:5001" };
                    Log.Warning("FIDO2_ORIGINS environment variable not set, defaulting to localhost URLs. This should be set for production deployments.");
                }
                else
                {
                    origins = originsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }
                
                options.ServerDomain = serverDomain;
                options.ServerName = "OpenALPR Webhook Processor";
                options.Origins = new HashSet<string>(origins);
                options.TimestampDriftTolerance = 300000;
                options.MDSCacheDirPath = "./config/mds-cache";
                
                Log.Information("FIDO2 configured with ServerDomain: {ServerDomain}, Origins: {Origins}", 
                    serverDomain, string.Join(", ", origins));
            });

            services.AddExternalServices();

            services.AddBackgroundServices(Configuration);

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

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };

            app.UseWebSockets(webSocketOptions);

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/index.html");
                endpoints.MapHub<ProcessorHub.ProcessorHub>("/api/processorHub")
                    .RequireAuthorization();
            });
        }
    }
}
