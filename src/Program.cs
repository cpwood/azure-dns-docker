using System;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DynamicAzureDns
{
    class Program
    {
        static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Console.WriteLine($"Azure Dynamic DNS - v{version}");
            
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder
                    .AddJsonFile("appsettings.debug.json", true)
                    .AddEnvironmentVariables())
                .ConfigureLogging(builder => builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddSimpleConsole(x =>  x.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]"))
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<Settings>(hostContext.Configuration);

                    services.AddSingleton<IValidator<Settings>, SettingsValidator>();
                    services.AddHostedService<Worker>();
                });
    }
}