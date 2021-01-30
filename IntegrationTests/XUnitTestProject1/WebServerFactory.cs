using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Users.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Integration
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<ProcessorContext>));

                services.Remove(descriptor);

                descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<UsersContext>));

                services.Remove(descriptor);

                services.AddDbContext<ProcessorContext>(options =>
                {
                    options.UseSqlite("Data Source=./processor.db");
                });

                services.AddDbContext<UsersContext>(options =>
                {
                    options.UseSqlite("Data Source=./users.db");
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var processorDb = scopedServices.GetRequiredService<ProcessorContext>();
                    var usersDb = scopedServices.GetRequiredService<UsersContext>();

                    processorDb.Database.EnsureCreated();
                    processorDb.Database.Migrate();

                    usersDb.Database.EnsureCreated();
                    usersDb.Database.Migrate();
                }
            });
        }
    }
}
