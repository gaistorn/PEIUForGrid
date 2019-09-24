using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
namespace PEIU.Hubbub
{
    public class Program
    {
        public static CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        private static IWebHost webHost = null;
        public static void Main(string[] args)
        {
            ////System.Console.CancelKeyPress += Console_CancelKeyPress;

            //AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            //System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += Default_Unloading;

            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                webHost = CreateWebHostBuilder(args).Build();
                
                webHost.Run();
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

        //private static void Default_Unloading(System.Runtime.Loader.AssemblyLoadContext obj)
        //{
        //    throw new NotImplementedException();
        //}

        private static async void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            CancellationTokenSource.Cancel();
            await webHost.StopAsync(CancellationTokenSource.Token);
        }

        private static async void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            //e.Cancel = true;
            CancellationTokenSource.Cancel();
            await webHost.StopAsync(CancellationTokenSource.Token);




        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseUrls(args.Length == 0 ? new string[] { "http://*:2121" } : args)
            //.UseUrls("http://*:2121")
                .UseStartup<Startup>()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                //logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseNLog();
    }
}
