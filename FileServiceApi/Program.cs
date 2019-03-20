using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using FileServiceApi.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace FileServiceApi
{
    public class Program
    {
        private static ConfigurationSettings _configurationSettings;

        public static void Main(string[] args)
        {
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == EnvironmentName.Development;

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(isDevelopment ? "appsettings-local.json" : "appsettings.json", optional: false)
                .Build();

            _configurationSettings = new ConfigurationSettings();
            config.GetSection("ConfigurationSettings").Bind(_configurationSettings);

            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo
                .File("logs//FileService.txt").CreateLogger();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services => services.AddAutofac())
                .UseStartup<Startup>();
    }
}
