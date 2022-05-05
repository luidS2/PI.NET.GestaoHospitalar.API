using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using System.IO;

namespace PI.GestaoHospitalar.Core.Extensions.Hosting
{
    public static class HostExtension
    {
        public static IHostBuilder ConfigureBase<TStartup>(this IHostBuilder host, string[] args)
            where TStartup : class
        {
            return host.ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddNLog();
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
                       webBuilder.UseStartup<TStartup>();
                   }).UseNLog();
        }
    }
}
