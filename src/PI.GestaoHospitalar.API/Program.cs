using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using NLog.Extensions.Logging;
using NLog.Web;
using System.IO;

namespace PI.GestaoHospitalar.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args)
                  .ConfigureLogging(logging =>
                  {
                      logging.ClearProviders();
                      // We have to be precise on the logging levels
                      logging.AddConsole();
                      logging.AddDebug();
                      logging.AddAzureWebAppDiagnostics();
                  })
                  .ConfigureServices(services =>
                  {
                      services.Configure<AzureFileLoggerOptions>(options =>
                      {
                          options.FileName = "my-azure-diagnostics-";
                          options.FileSizeLimit = 50 * 1024;
                          options.RetainedFileCountLimit = 5;
                      });
                  })
                  .ConfigureAppConfiguration((hostingContext, config) =>
                  {
                      var env = hostingContext.HostingEnvironment;

                      config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                      config.AddEnvironmentVariables();
                      if (File.Exists($"nlog.{env.EnvironmentName}.config"))
                      {
                          NLogBuilder.ConfigureNLog($"nlog.{env.EnvironmentName}.config");
                      }
                  })
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseStartup<Startup>()
                               .ConfigureKestrel((context, options) =>
                               {
                                   options.Limits.MaxRequestBodySize = null;
                               });
                  }).UseNLog();
    }
}
