using PEIU.Models;
using Microsoft.Extensions.Configuration;
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
using PEIU.DataServices;

namespace PVMeasure
{
    public class PVBackgroundService
    {
        NLog.ILogger logger;
        IConfiguration config;
        MqttConfig mqtt_config;
        IDatabase redis;
        int SiteId;
        MqttClientProxyCollection mqtt_clients;
        ISessionFactory sessionFactory;
        Dictionary<long, string> stamp_map = new Dictionary<long, string>();
        TimeSpan PollInterval;
        string DeviceName = "";
        string redisDeviceName = "";
        public PVBackgroundService(NLog.ILogger loggerFactory, 
            MqttClientProxyCollection mqttClientProxies, RedisConnectionFactory redisFactory, 
            IConfiguration configuration)
        {
            logger = loggerFactory;
            config = configuration;
            mqtt_clients = mqttClientProxies;
            mqtt_config = configuration.GetSection("MQTTBrokers").Get<MqttConfig>();
            SiteId = configuration.GetSection("SiteId").Get<int>();
            string mssql_conn = configuration.GetConnectionString("mssql");
            PollInterval = configuration.GetSection("PollInterval").Get<TimeSpan>();
            redis = redisFactory.Connection().GetDatabase(1);
            DeviceName = configuration.GetSection("DeviceName").Get<string>();
            redisDeviceName = configuration.GetSection("RedisKeyName").Get<string>();
            sessionFactory = new MsSqlAccessManager().CreateSessionFactory(mssql_conn);
        }

        public async Task RunWorkerAsync(CancellationToken Token)
        {
            DateTime nextMeasure = DateTime.MinValue;
            while (!Token.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.Now < nextMeasure)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    nextMeasure = DateTime.Now.Add(PollInterval);

                    using (var session = sessionFactory.OpenSession())
                    {
                        var invt_list = await session.CreateCriteria<INVERTER_DATA_RT>().ListAsync<INVERTER_DATA_RT>();
                        foreach (INVERTER_DATA_RT rt in invt_list)
                        {
                            string date_key = rt.ID_DATE.ToString("yyyyMMddHHmmss");
                            if (stamp_map.ContainsKey(rt.ID_CODE) && stamp_map[rt.ID_CODE] == date_key)
                                continue;
                            if (stamp_map.ContainsKey(rt.ID_CODE) == false)
                            {
                                stamp_map.Add(rt.ID_CODE, date_key);
                            }
                            else
                                stamp_map[rt.ID_CODE] = date_key;
                            int idCode = int.Parse(rt.ID_CODE.ToString()) + 1;
                            string redis_key = $"{redisDeviceName}{idCode}";
                            string mqtt_key = $"{DeviceName}{idCode}";
                            logger.Trace($"READING INVERTER DB {rt.ID_DATE} deviceId: {redis_key}");
                            HashEntry[] hashEntries = null;
                            var jRow = CreateJObject(rt, redis_key, out hashEntries);
                            
                            await redis.HashSetAsync(redis_key, hashEntries);

                            string topic = $"hubbub/{SiteId}/{mqtt_key}/AI";

                            bool IsSuccess = false;
                            foreach (var mqtt_proxy in mqtt_clients)
                            {
                                try
                                {

                                    var msg = CreateMqttMessage(topic, jRow.ToString(), mqtt_proxy.Options.QosLevel);
                                    var mqtt_client = mqtt_proxy.MqttClient;

                                    if (mqtt_client.IsConnected == false)
                                    {
                                        logger.Trace($"BROKER IS NOT CONNECTED {rt.ID_DATE} deviceId: {redis_key}");
                                        continue;
                                    }
                                    await mqtt_client.PublishAsync(msg);
                                    logger.Trace($"SENDING QUEUE {rt.ID_DATE} deviceId: {redis_key}");
                                    IsSuccess = true;

                                    //if (mqtt_client.IsConnected)
                                    //{

                                    //}
                                    //else
                                    //{
                                    //    logger.LogError($"MQTT has not connected. {mqtt_client.Options.ChannelOptions}");

                                    //}
                                    Thread.Sleep(10);
                                    //break;
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, ex.Message);
                                }
                            }

                            if (IsSuccess)
                                logger.Info("Sending Queue\n" + jRow.ToString());

                        }
                    }
                    
                }
                catch(Exception ex)
                {
                    logger.Error(ex, ex.Message);
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

        private JObject CreateJObject(INVERTER_DATA_RT invert, string deviceName, out HashEntry[] hashEntries)
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
            datarow.Add("groupid", 4);
            datarow.Add("groupname", "PV_SYSTEM");
            datarow.Add("deviceId", deviceName);
            datarow.Add("siteId", SiteId);
            entries.Add(new HashEntry("timestamp", timeStamp.ToString()));
            datarow.Add("timestamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
            hashEntries = entries.ToArray();
            return datarow;
        }
    }
}
