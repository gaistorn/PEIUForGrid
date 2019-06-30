using DataModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using Newtonsoft.Json.Linq;
using NModbus;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Hubbub.Services
{
    public class ModbusBackgroundService : BackgroundService
    {
        ILogger<ModbusBackgroundService> logger;
        IModbusFactory modbusFactory;
        IConfiguration config;
        MqttConfig mqtt_config;
        ModbusSystem modbus;
        IDatabase redis;
        MqttClientProxyCollection mqtt_clients;
        int SiteId = 0;
        readonly static ConcurrentDictionary<int, DateTime> dtMap = new ConcurrentDictionary<int, DateTime>();
        private readonly object locker = new object();
        IMemoryCache cache;
        public ModbusBackgroundService(ILoggerFactory loggerFactory, IModbusFactory modbus_factory,
            MqttClientProxyCollection mqttClientProxies, IRedisConnectionFactory redisFactory,
            IConfiguration configuration, ModbusSystem modbusSystem, IMemoryCache memoryCache)
        {
            logger = loggerFactory.CreateLogger<ModbusBackgroundService>();
            config = configuration;
            modbusFactory = modbus_factory;
            mqtt_clients = mqttClientProxies;
            mqtt_config = configuration.GetSection("MQTTBrokers").Get<MqttConfig>();
            SiteId = configuration.GetSection("SiteId").Get<int>();
            modbus = modbusSystem;
            cache = memoryCache;
            redis = redisFactory.Connection().GetDatabase(1);
        }

       

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                //Console.WriteLine($"Entry TokenCancel: {stoppingToken.IsCancellationRequested}");
                if (stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine($"Stop Phase3. TokenCancel: {stoppingToken.IsCancellationRequested}");
                    break;
                }
                stoppingToken.ThrowIfCancellationRequested();
                //var parallelResult = Parallel.ForEach<GroupPoint>(modbus.GroupPoints, (async x =>
                foreach (var x in modbus.GroupPoints)
                {
                    if (x.Disable == 1)
                        continue;
                    if (dtMap.ContainsKey(x.GroupId) == false)
                        dtMap[x.GroupId] = DateTime.MinValue;
                    if (DateTime.Now > dtMap[x.GroupId])
                    {
                        dtMap[x.GroupId] = DateTime.Now.Add(TimeSpan.FromSeconds(x.PollIntervalSec));
                        bool isConnected = modbusFactory.ReconnectWhenDisconnected();
                        if (isConnected == false)
                        {
                            logger.LogWarning("모드버스 접속에 실패했습니다.");
                            dtMap[x.GroupId] = DateTime.Now.Add(TimeSpan.FromSeconds(x.RetryIntervalSec));
                            continue;
                        }
                        HashEntry[] hashEntries = null;
                        JObject parentModel = modbusFactory.ReadModbusToJson(SiteId,  x, out hashEntries);
                        if (parentModel == null)
                        {
                            dtMap[x.GroupId] = DateTime.Now.Add(TimeSpan.FromSeconds(x.RetryIntervalSec));
                            continue;
                        }

                        string redis_key = $"{modbus.DeviceName}";
                        await redis.HashSetAsync(redis_key, hashEntries);
                        string topic = $"hubbub/{SiteId}/{modbus.DeviceName}/AI";
                        foreach (var mqtt_proxy in mqtt_clients)
                        {
                            try
                            {

                                var msg = CreateMqttMessage(topic, parentModel.ToString(), mqtt_proxy.Options.QosLevel);
                                var mqtt_client = mqtt_proxy.MqttClient;

                                if (mqtt_client.IsConnected == false)
                                    continue;
                                await mqtt_client.PublishAsync(msg);
                                
                                //if (mqtt_client.IsConnected)
                                //{
                                   
                                //}
                                //else
                                //{
                                //    logger.LogError($"MQTT has not connected. {mqtt_client.Options.ChannelOptions}");
                                    
                                //}
                                Thread.Sleep(10);
                                break;
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, ex.Message);
                            }
                        }


                    }

                    Thread.Sleep(10);

                    //}));
                }
                //Console.WriteLine("ForEach Working...");
                //while (true)
                //{
                //    if (parallelResult.IsCompleted)
                //        break;
                //    Thread.Sleep(10);
                //}

                //Console.WriteLine("ForEach Completed");
                Thread.Sleep(1);
                //}
                //managedClient.sub
            }
            //Console.WriteLine("ExecuteAsync End");
            //foreach (var client in mqtt_clients)
            //    await client.MqttClient.DisconnectAsync();
            //modbusFactory.Dispose();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Completed");
            foreach (var client in mqtt_clients)
                await client.MqttClient.DisconnectAsync();
            modbusFactory.Dispose();
        }

        private MqttApplicationMessage CreateMqttMessage(string topic, string payload, int qos)
        {
            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(payload);
            var applicationMessage = new MqttApplicationMessageBuilder()
                       .WithTopic(topic)
                       .WithPayload(payload_buffer)
                       .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
                       .Build();
            return applicationMessage;
        }


        private async Task< IMqttClient[]> TryInitializeMqtt(MqttConfig config)
        {
            List<IMqttClient> result = new List<IMqttClient>();
            int idx = 0;
            foreach(MqttAddress addr in config.DataBrokerAddress)
            {
                IMqttClient client = await CreateMqttClient(addr);
                if (client != null)
                    result.Add(client);
            }
            return result.ToArray();
        }

        private MqttClientOptions CreateMqttOption(MqttAddress addr)
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
            return ClientOptions;
        }

        private async Task<IMqttClient> CreateMqttClient(MqttAddress addr)
        {

            var ClientOptions = CreateMqttOption(addr);
            var mqttClient = new MqttFactory().CreateMqttClient();
            mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(ManagedClient_ApplicationMessageReceived);
            mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(ManagedClient_Connected);
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(ManagedClient_Disconnected);

            bool IsSuccess = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await mqttClient.ConnectAsync(ClientOptions);
                    IsSuccess = true;
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "#### MQTT BROKER CONNECTING FAILED ####");
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    continue;
                }
            }

            if (IsSuccess == false)
            {
                logger.LogError("#### 브로커 접속에 실패했습니다. 다시 실행해주세요. ####");
                return null;
            }
            return mqttClient;
        }

        private async void RetryConnected(MqttClientProxy proxy)
        {
            var ClientOptions = CreateMqttOption(proxy.Options);
            await proxy.MqttClient.ConnectAsync(ClientOptions);
        }

        private async Task ManagedClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            
        }

        private async Task ManagedClient_Connected(MqttClientConnectedEventArgs e)
        {

        }

        private async Task ManagedClient_Disconnected(MqttClientDisconnectedEventArgs e)
        {
            string errorMEssage = $"MQTT Disconnected. ErrorCode = 0x{e.AuthenticateResult.ResultCode:X} - {e.AuthenticateResult.ResultCode.ToString()}";
            logger.LogWarning(errorMEssage);
            if(e.Exception != null)
            {
                logger.LogError(e.Exception, e.Exception.Message);
            }
            
        }

    }
}
