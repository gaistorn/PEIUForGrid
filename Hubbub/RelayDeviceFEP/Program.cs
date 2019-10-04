using EasyModbus;
using FireworksFramework.Mqtt;
using NLog;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelayDeviceFEP
{
    class Program
    {
        static CancellationTokenSource token;
        static ILogger logger;
        static void Main(string[] args)
        {
            token = new CancellationTokenSource();
            System.Console.CancelKeyPress += Console_CancelKeyPress;
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            logger = NLog.LogManager.Configuration.LogFactory.GetLogger("");

            AppSetting setting = new AppSetting("appsettings.json");
            ModbusConfig pvmodbusConfig = setting.GetSection<ModbusConfig>("PVModbus");
            ModbusConfig essmodbusConfig = setting.GetSection<ModbusConfig>("ESSModbus");
            var redisConfiguration = setting.GetSection<RedisConfiguration>("redis");
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);
            MysqlDataAccess access = new MysqlDataAccess(setting.GetConnectionString("etridb"));
            RelayDataInformation relayDataInformation = setting.GetSection<RelayDataInformation>("Information");

            try
            {
                using (RelayDevicePublisher publisher = new RelayDevicePublisher(relayDataInformation, pvmodbusConfig, essmodbusConfig, access, connectionMultiplexer, logger))
                {
                    publisher.Initialize();
                    Task t = publisher.RunningAsync(token.Token);
                    t.Wait();
                }

            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            logger.Warn("Terminating...");
            e.Cancel = true;
            token.Cancel();
        }
    }
}
