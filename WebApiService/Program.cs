using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FireworksFramework.Mqtt;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
namespace PEIU.Service.WebApiService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Services.HTMLGenerator gen = new Services.HTMLGenerator();
            //gen.GenerateHtml("NotifyEmail.html", new { Company = "대건소프트", Name = "김기룡" });
             // NLog: setup the logger first to catch all errors
             var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            try
            {
                logger.Debug("init main");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                 .UseKestrel()
                .UseSetting("https_port", "3021")
                 .UseUrls("https://*:3021;http://*:3011")
                 //.UseUrls("http://*:3011")
                .UseStartup<Startup>()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                //logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseNLog();
    }
}
