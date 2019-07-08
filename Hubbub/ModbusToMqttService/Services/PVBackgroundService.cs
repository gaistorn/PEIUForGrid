using DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using NHibernate;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Hubbub.Services
{
    public class PVBackgroundService : BackgroundService
    {
        ILogger<PVBackgroundService> logger;
        IConfiguration config;
        MqttConfig mqtt_config;
        IDatabase redis;
        int SiteId;
        MqttClientProxyCollection mqtt_clients;
        ISessionFactory sessionFactory;
        Dictionary<long, long> stamp_map = new Dictionary<long, long>();
        string DeviceName = "";
        public PVBackgroundService(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, IModbusFactory modbus_factory,
            MqttClientProxyCollection mqttClientProxies, IRedisConnectionFactory redisFactory,
            IConfiguration configuration)
        {
            logger = loggerFactory.CreateLogger<PVBackgroundService>();
            config = configuration;
            mqtt_clients = mqttClientProxies;
            mqtt_config = configuration.GetSection("MQTTBrokers").Get<MqttConfig>();
            SiteId = configuration.GetSection("SiteId").Get<int>();
            string mssql_conn = configuration.GetConnectionString("mssql");
            redis = redisFactory.Connection().GetDatabase(1);
            DeviceName = configuration.GetSection("Modbus:DeviceName").Get<string>();
            DeviceName = DeviceName.Substring(0, DeviceName.Length - 1);
            sessionFactory = new MsSqlAccessManager().CreateSessionFactory(mssql_conn);
        }

        protected override async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
           
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var session = sessionFactory.OpenStatelessSession())
                    {
                        var invt_list = await session.CreateCriteria<INVERTER_DATA_RT>().ListAsync<INVERTER_DATA_RT>();
                        foreach (INVERTER_DATA_RT rt in invt_list)
                        {
                            if (stamp_map.ContainsKey(rt.ID_CODE) && stamp_map[rt.ID_CODE] == rt.ID_DATE.Ticks)
                                continue;
                            if (stamp_map.ContainsKey(rt.ID_CODE) == false)
                            {
                                stamp_map.Add(rt.ID_CODE, rt.ID_DATE.Ticks);
                            }
                            else
                                stamp_map[rt.ID_CODE] = rt.ID_DATE.Ticks;

                            string redis_key = $"{DeviceName}{rt.ID_CODE + 1}";

                            HashEntry[] hashEntries = null;
                            var jRow = CreateJObject(rt, out hashEntries);
                            await redis.HashSetAsync(redis_key, hashEntries);

                            string topic = $"hubbub/{SiteId}/{redis_key}/AI";
                            foreach (var mqtt_proxy in mqtt_clients)
                            {
                                try
                                {

                                    var msg = CreateMqttMessage(topic, jRow.ToString(), mqtt_proxy.Options.QosLevel);
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
                    }
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
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

        private JObject CreateJObject(INVERTER_DATA_RT invert, out HashEntry[] hashEntries)
        {
            List<HashEntry> entries = new List<HashEntry>();
            PropertyInfo[] propertyInfos = typeof(INVERTER_DATA_RT).GetProperties();
            foreach(PropertyInfo pi in propertyInfos)
            {
                string name = pi.Name;
                object value = pi.GetValue(invert);
                string str_Value = value == null ? "0" : value.ToString();
                entries.Add(new HashEntry(name, str_Value));
            }
            DateTime timeStamp = DateTime.Now;
            JObject datarow = JObject.FromObject(invert);
            datarow.Add("groupid", 3);
            datarow.Add("groupname", "PV_SYSTEM");
            datarow.Add("deviceId", DeviceName);
            datarow.Add("siteId", SiteId);
            entries.Add(new HashEntry("timestamp", timeStamp.ToString()));
            datarow.Add("timestamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
            hashEntries = entries.ToArray();
            return datarow;
        }
    }
}
