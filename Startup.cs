using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlate;
using OpenAlprWebhookProcessor.Users;
using OpenAlprWebhookProcessor.Users.Data;
using OpenAlprWebhookProcessor.Users.Register;
using OpenAlprWebhookProcessor.WebhookProcessor;
using Serilog;
using System;
using System.Linq;
using System.Text;

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
            services.AddControllersWithViews();

            var jwtConfiguration = new JwtConfiguration();
            Configuration.GetSection("Jwt").Bind(jwtConfiguration);
            var key = Encoding.ASCII.GetBytes(jwtConfiguration.SecretKey);

            services.AddSingleton(jwtConfiguration);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddScoped<IUserService, UserService>();

            var agentConfiguration = new AgentConfiguration();
            Configuration.GetSection("OpenAlprAgent").Bind(agentConfiguration);

            if (agentConfiguration.Cameras == null || agentConfiguration.Cameras.Count == 0)
            {
                throw new ArgumentException("no cameras found in appsettings, check your configuration");
            }

            services.AddLogging(config =>
            {
                config.AddDebug();
                config.AddConsole();
            });

            services.AddDbContext<ProcessorContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("ProcessorContext")));

            services.AddDbContext<UsersContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("UsersContext")));


            services.AddSingleton(agentConfiguration);

            services.AddScoped<WebhookHandler>();
            services.AddScoped<GetLicensePlateHandler>();

            services.AddSingleton<CameraUpdateService.CameraUpdateService>();
            services.AddSingleton<IHostedService>(p => p.GetService<CameraUpdateService.CameraUpdateService>());

            services.AddSwaggerGen();

            var mapper = new MapperConfiguration(mc =>
            {
                mc.CreateMap<User, UserModel>();
                mc.CreateMap<User, UserModel>();
                mc.CreateMap<RegisterModel, User>();
                mc.CreateMap<UpdateModel, User>();
            });

            services.AddSingleton(mapper.CreateMapper());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            UsersContext usersContext)
        {
            MigrateDatabases(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenALPR Webhook Processor");
                c.RoutePrefix = "/swagger";
            });

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        private static void MigrateDatabases(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                if (processorContext.Database.GetPendingMigrations().Any())
                {
                    processorContext.Database.Migrate();
                }

                var usersContext = scope.ServiceProvider.GetRequiredService<UsersContext>();

                if (usersContext.Database.GetPendingMigrations().Any())
                {
                    usersContext.Database.Migrate();
                }
            }
        }
    }
}
