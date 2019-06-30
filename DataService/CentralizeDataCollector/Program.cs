using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.DataServices
{
    class Program
    {
        static CollectWorker mqtt_worker;
        static BackgroundCassandraWorker cassandraWorker;
        static CancellationTokenSource cancellationTokenSource;
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .Build();
            // NLog: setup the logger first to catch all errors
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
            NLog.ILogger logger = NLog.LogManager.Configuration.LogFactory.GetLogger("");
            CassandraConfiguration cassandra_config = config.GetSection("cassandra").Get<CassandraConfiguration>();
            MqttAddress data_mqtt_address = config.GetSection("MQTTBrokers:DataBrokerAddress").Get<MqttAddress>();
            cancellationTokenSource = new CancellationTokenSource();
            IBackgroundTaskQueue<JObject> dataQueue = new BackgroundTaskQueue<JObject>();
             mqtt_worker = new CollectWorker(dataQueue);
            Console.CancelKeyPress += Console_CancelKeyPress;
            cassandraWorker = new BackgroundCassandraWorker(logger, cancellationTokenSource, cassandra_config, dataQueue);
            cassandraWorker.RunWorkerCompleted += CassandraWorker_RunWorkerCompleted;
            
            Task t = mqtt_worker.ConnectionAsync(data_mqtt_address.ClientId, data_mqtt_address.BindAddress, data_mqtt_address.Port, (ushort)data_mqtt_address.QosLevel, data_mqtt_address.Topic);
            t.Wait();
            Task worker_Task = cassandraWorker.RunWorkerAsync();
            worker_Task.Wait();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("Terminating...");
            cancellationTokenSource.Cancel();

        }

        private static void CassandraWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mqtt_worker.Dispose();
            Console.WriteLine("Completed");
        }
    }
}
