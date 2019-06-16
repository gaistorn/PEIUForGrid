using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PES.Service.DataService
{
    public class Program
    {
        public static NLog.Logger NLogger = null;
        public static void Main(string[] args)
        {
            try
            {
                NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
                NLogger = NLog.LogManager.Configuration.LogFactory.GetLogger("");
                //LogFactory = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
                //var logger = LogFactory.GetCurrentClassLogger();
                //logger.Info("start");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                NLogger.Error(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                 //.UseKestrel()
                 .UseUrls("http://*:16000")
                .UseStartup<Startup>();
                //.ConfigureLogging(logging =>
                //{
                //    logging.ClearProviders();
                //    logging.SetMinimumLevel(LogLevel.Trace);
                //}).UseNLog();
    }
}
