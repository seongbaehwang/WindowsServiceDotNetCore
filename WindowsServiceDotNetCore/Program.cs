using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindowsServiceDotNetCore.Entities;
using WindowsServiceDotNetCore.Quartz;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;
using Serilog.Formatting.Json;

namespace WindowsServiceDotNetCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "DOTNET_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var hostingEnvironment = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true);

                    if (hostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>(optional: true);
                    }
                    config.AddEnvironmentVariables();
                    if (args == null)
                        return;
                    config.AddCommandLine(args);
                })
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    var option = hostingContext.Configuration.GetSection(nameof(RollingFileSinkOptions))
                        .Get<RollingFileSinkOptions>();

                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                        .WriteTo.RollingFile(formatter: new JsonFormatter(renderMessage: true),
                            pathFormat: option.PathFormat,
                            retainedFileCountLimit: option.RetainedFileCountLimit)
                        ;

                    if (!isService)
                    {
                        loggerConfiguration.WriteTo.Console();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddDbContext<HeroContext>(options => options
                    //    .UseSqlServer(hostContext.Configuration.GetConnectionString(nameof(HeroContext)),
                    //        // default retry is 6, read document first before using
                    //        providerOption => providerOption.EnableRetryOnFailure())
                    //    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    //    // ** Testing purpose for now
                    //    .EnableSensitiveDataLogging());

                    // For in memory database, once connection is closed, all data will be lost, so a connection is created explicitly
                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();

                    var dbContextOptions = new DbContextOptionsBuilder<HeroContext>()
                        .UseSqlite(connection)
                        .Options;

                    services.AddDbContext<HeroContext>(options => options
                        .UseSqlite(connection));
                    
                    using (var heroContext = new HeroContext(dbContextOptions))
                    {
                        heroContext.Database.EnsureCreated();
                    }

                    services.AddHostedService<FileWriterService>();

                    // Add Quartz services

                    // ***** Use this factory only for singleton or transient jobs
                    //services.AddSingleton<IJobFactory, SingletonJobFactory>();

                    // ***** Each job runs under a scope. This factory support all types job
                    services.AddSingleton<IJobFactory, ScopedJobFactory>();

                    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

                    // Add our job
                    //services.AddSingleton<HelloWorldJob>();
                    services.AddScoped<HelloWorldJob>();
                    //services.AddTransient<HelloWorldJob>();
                    services.AddScoped<ScopedObject>();

                    services.AddSingleton<HelloWorldJob2>();
                    services.AddTransient<HeroJob>();

                    var schedules = hostContext.Configuration.GetSection("JobSchedules").Get<Dictionary<string, string[]>>();

                    // ** Enable as required
                    //services.AddSingleton(new JobSchedule(typeof(HelloWorldJob), schedules[nameof(HelloWorldJob)]));
                    //services.AddSingleton(new JobSchedule(typeof(HelloWorldJob2), schedules[nameof(HelloWorldJob2)]));
                    services.AddSingleton(new JobSchedule(typeof(HeroJob), schedules[nameof(HeroJob)]));

                    services.AddHostedService<QuartzHostedService>();

                });

            if (isService)
            {
                await builder.RunAsServiceAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
    }
}
