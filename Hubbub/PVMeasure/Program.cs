using DataModel;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using NHibernate;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PVMeasure
{
    class Program
    {
        static NLog.ILogger logger;
        static CancellationTokenSource source;
        static void Main(string[] args)
        {

            IConfiguration config = new ConfigurationBuilder()
         .AddJsonFile("appsettings.json", true, true)
         .Build();
            // NLog: setup the logger first to catch all errors
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");
            logger = NLog.LogManager.Configuration.LogFactory.GetLogger("");
            var redisConfiguration = config.GetSection("redis").Get<RedisConfiguration>();
            var mqtt_informations = config.GetSection("MQTTBrokers").Get<MqttConfig>();
            RedisConnectionFactory redisConnectionFactory = new RedisConnectionFactory(redisConfiguration);
            string mssql_conn = config.GetConnectionString("mssql");
            ISessionFactory sessionFactory = new MsSqlAccessManager().CreateSessionFactory(mssql_conn);
            var proxy = TryInitializeMqtt(mqtt_informations);
            source = new CancellationTokenSource();
            PVBackgroundService service = new PVBackgroundService(logger, proxy, redisConnectionFactory, config);
            Task worker = service.RunWorkerAsync(source.Token);
            worker.Wait();
        }

        private static MqttClientProxyCollection TryInitializeMqtt(MqttConfig config)
        {
            MqttClientProxyCollection result = new MqttClientProxyCollection();
            int idx = 0;
            foreach (MqttAddress addr in config.DataBrokerAddress)
            {
                IMqttClient client = CreateMqttClient(addr);
                if (client != null)
                    result.Add(new MqttClientProxy(client, addr));
            }
            return result;
        }

        private static IMqttClient CreateMqttClient(MqttAddress addr)
        {
            var ClientOptions = new MqttClientOptions
            {
                ClientId = addr.ClientId,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = addr.BindAddress,
                    Port = addr.Port,

                }

            };

            var mqttClient = new MqttFactory().CreateMqttClient();

            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
            {
                //Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(1));

                try
                {
                    await mqttClient.ConnectAsync(ClientOptions);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });
            bool IsSuccess = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    mqttClient.ConnectAsync(ClientOptions);
                    IsSuccess = true;
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"#### MQTT BROKER CONNECTING FAILED ### \n{addr.ToJson()}");
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    continue;
                }
            }

            if (IsSuccess == false)
            {
                logger.Error("#### 브로커 접속에 실패했습니다. 다시 실행해주세요. ####");
                return null;
            }
            return mqttClient;
        }
    }
}
