using Microsoft.Extensions.Configuration;
using PEIU.Core;
using PEIU.DataServices;
using PEIU.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventTranslator
{
    class Program
    {
        readonly static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
         .AddJsonFile("appsettings.json", true, true)
         .Build();

            // NLog: setup the logger first to catch all errors
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
            NLog.ILogger logger = NLog.LogManager.Configuration.LogFactory.GetLogger("");

            string connection_string = config.GetConnectionString("mysql");

            IBackgroundTaskQueue<EventSummary> queue = new BackgroundTaskQueue<EventSummary>();
            EventHavestor eventHavestor = new EventHavestor(queue);

            MqttAddress queue_address = config.GetSection("MQTTBrokers").Get<MqttAddress>();
            Console.CancelKeyPress += Console_CancelKeyPress;

            EventRecorder eventRecorder = new EventRecorder(logger, queue, connection_string);
            
            Task T = eventHavestor.ConnectionAsync(queue_address.ClientId, queue_address.BindAddress, queue_address.Port, queue_address.QosLevel, queue_address.Topic);
            T.Wait();
            Task t1 = eventRecorder.RunAsync(cancellationTokenSource.Token);
            t1.Wait();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        }
    }
}
