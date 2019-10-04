using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireworksFramework.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Python.Runtime;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace EtriCommandAgent
{
    public class Program
    {
        public static bool ForceMode = false;
        public static void Main(string[] args)
        {
            //PythonEngine.Initialize();
            var logger = LogManager.GetCurrentClassLogger();
            ForceMode = args.Length > 0 && args[0] == "FORCEMODE";
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            CreateHostBuilder(args).Build().Run();
            //PythonEngine.Shutdown();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {

                    var redisConfiguration = hostContext.Configuration.GetSection("redis").Get<RedisConfiguration>();
                    ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);
                    services.AddSingleton(connectionMultiplexer);
                    services.AddSingleton<MysqlDataAccessSingleton>();
                    services.AddSingleton<EtriCommandPublisher>();
                    services.AddHostedService<Worker>();
                    services.AddLogging(loggingBuilder =>
                    {
                        // configure Logging with NLog
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                        loggingBuilder.AddNLog();
                    });
                });
    }
}
