using EasyModbus;
using FireworksFramework.Mqtt;
using NLog;
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
            ModbusConfig modbusConfig = setting.GetSection<ModbusConfig>("Modbus");
            RelayDataInformation relayDataInformation = setting.GetSection<RelayDataInformation>("Information");

            try
            {
                using (RelayDevicePublisher publisher = new RelayDevicePublisher(relayDataInformation, modbusConfig, logger))
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
