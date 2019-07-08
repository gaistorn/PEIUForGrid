using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MqttProxy
{
   
    class Program
    {
        static SubscribeWorker mqtt_worker;
        static CancellationTokenSource cancellationTokenSource = null;
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .Build();
            // NLog: setup the logger first to catch all errors
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
            NLog.ILogger logger = NLog.LogManager.Configuration.LogFactory.GetLogger("");
            var from_mqtt_address =  config.GetSection("MQTTBrokers:From").Get<MqttAddress>();
            var to_mqtt_address = config.GetSection("MQTTBrokers:To").Get<MqttAddress>();
            var topic = config.GetSection("SubscribeTopic").Get<string>();
            cancellationTokenSource = new CancellationTokenSource();
            mqtt_worker = new SubscribeWorker(logger, topic, to_mqtt_address);
            Console.CancelKeyPress += Console_CancelKeyPress;
            Task t = mqtt_worker.ConnectionAsync(from_mqtt_address.ClientId, from_mqtt_address.BindAddress, from_mqtt_address.Port, from_mqtt_address.QosLevel, topic);
            t.Wait();
            var token = cancellationTokenSource.Token;
            while (token.IsCancellationRequested == false)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Complete");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {

            e.Cancel = true;
            cancellationTokenSource.Cancel();

        }
    }
}
