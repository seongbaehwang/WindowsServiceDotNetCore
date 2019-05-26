using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WindowsServiceDotNetCore.Quartz;
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
                    var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
                    var hostingEnvironment = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true);

                    if (hostingEnvironment.IsDevelopment() && entryAssemblyName != null)
                    {
                        // ** hostingEnvironment.ApplicationName is not set at this point
                        //var assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                        var assembly = Assembly.Load(new AssemblyName(entryAssemblyName));
                        if (assembly != null)
                            config.AddUserSecrets(assembly, true);
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
                    services.AddHostedService<FileWriterService>();

                    // Add Quartz services
                    services.AddSingleton<IJobFactory, SingletonJobFactory>();
                    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

                    // Add our job
                    //services.AddSingleton<HelloWorldJob>();
                    // ** AddTransient is used to check SingletonJobFactory.ReturnJob
                    services.AddTransient<HelloWorldJob>();

                    services.AddSingleton(new JobSchedule(
                        jobType: typeof(HelloWorldJob),
                        cronExpression: "0/5 * * * * ?")); // run every 5 seconds

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
