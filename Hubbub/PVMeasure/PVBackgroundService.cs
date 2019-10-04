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
        readonly ISessionFactory mysqlSessionFactory;
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
            string etri_conn = configuration.GetConnectionString("etridb");
            PollInterval = configuration.GetSection("PollInterval").Get<TimeSpan>();
            redis = redisFactory.Connection().GetDatabase(1);
            DeviceName = configuration.GetSection("DeviceName").Get<string>();
            redisDeviceName = configuration.GetSection("RedisKeyName").Get<string>();
            sessionFactory = new MsSqlAccessManager().CreateSessionFactory(mssql_conn);
            mysqlSessionFactory = new MySqlAccessManager(etri_conn, Assembly.GetExecutingAssembly()).SessionFactory;
        }

        private async Task InsertTbPv(float[] pvPowers, float[] pvtotal, float[] pvtodays, CancellationToken token)
        {
            try
            {
                using(var session = mysqlSessionFactory.OpenStatelessSession())
                using(var trans = session.BeginTransaction())
                {
                    TbPv pv = new TbPv();
                    pv.Date = DateTime.Now;
                    pv.Energy1 = pvtotal[0];
                    pv.Energy2 = pvtotal[1];
                    pv.Energy3 = pvtotal[2];
                    pv.Energy4 = pvtotal[3];

                    pv.Power1 = pvPowers[0];
                    pv.Power2 = pvPowers[1];
                    pv.Power3 = pvPowers[2];
                    pv.Power4 = pvPowers[3];

                    pv.Pv1_Today_Eng = pvtodays[0];
                    pv.Pv2_Today_Eng = pvtodays[1];
                    pv.Pv3_Today_Eng = pvtodays[2];
                    pv.Pv4_Today_Eng = pvtodays[3];
                    await session.InsertAsync(pv);
                    await trans.CommitAsync(token);
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
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
                    float[] pv_engs = new float[4];
                    float[] pv_powers = new float[4];
                    float[] pv_todays = new float[4];
                    using (var session = sessionFactory.OpenSession())
                    {
                        var invt_list = await session.CreateCriteria<INVERTER_DATA_RT>().ListAsync<INVERTER_DATA_RT>();
                        foreach (INVERTER_DATA_RT rt in invt_list)
                        {
                            string date_key = rt.ID_DATE.ToString("yyyyMMddHHmmss");
                            int idCode = int.Parse(rt.ID_CODE.ToString()) + 1;
                            pv_engs[idCode - 1] = rt.TOTAL_POWER.Value;
                            pv_powers[idCode - 1] = rt.ID_POWER.Value;
                            pv_todays[idCode - 1] = rt.TODAY_POWER.Value;

                            if (stamp_map.ContainsKey(rt.ID_CODE) && stamp_map[rt.ID_CODE] == date_key)
                                continue;
                            if (stamp_map.ContainsKey(rt.ID_CODE) == false)
                            {
                                stamp_map.Add(rt.ID_CODE, date_key);
                            }
                            else
                                stamp_map[rt.ID_CODE] = date_key;
                            



                            string redis_key = $"{SiteId}.{redisDeviceName}{idCode}";
                            string mqtt_key = $"PV{idCode}";
                            logger.Trace($"READING INVERTER DB {rt.ID_DATE} deviceId: {redis_key}");
                            HashEntry[] hashEntries = null;
                            var jRow = CreateJObject(rt, $"PV{idCode}", out hashEntries);
                            
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

                    await InsertTbPv(pv_powers, pv_engs, pv_todays, Token);

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
            foreach (PropertyInfo pi in propertyInfos)
            {
                string name = pi.Name;
                object value = pi.GetValue(invert);
                string str_Value = value == null ? "0" : value.ToString();
                entries.Add(new HashEntry(name, str_Value));
            }
            DateTime timeStamp = DateTime.Now;
            JObject datarow = new JObject();//JObject.FromObject(invert);
            datarow.Add("groupid", 4);
            datarow.Add("groupname", "PV_SYSTEM");
            datarow.Add("deviceId", deviceName);
            datarow.Add("siteId", SiteId);
            datarow.Add("normalizedeviceid", deviceName);
            entries.Add(new HashEntry("timestamp", timeStamp.ToString()));
            datarow.Add("timestamp", invert.ID_DATE.ToString("yyyyMMddHHmmss"));
            datarow.Add("TotalActivePower", invert.ID_POWER);
            datarow.Add("TotalReactivePower", 0);
            datarow.Add("ReverseActivePower", 0);
            datarow.Add("ReverseReactivePower", 0);
            datarow.Add("vltR", invert.A_VOLTAGE);
            datarow.Add("vltS", invert.B_VOLTAGE);
            datarow.Add("vltT", invert.C_VOLTAGE);
            datarow.Add("crtR", invert.A_CURRENT);
            datarow.Add("crtS", invert.B_CURRENT);
            datarow.Add("crtT", invert.C_CURRENT);
            datarow.Add("Frequency", invert.FREQUENCY);
            datarow.Add("EnergyTotalActivePower", invert.TOTAL_POWER);
            datarow.Add("EnergyTodayActivePower", invert.TODAY_POWER);
            datarow.Add("EnergyTotalReactivePower", 0);
            datarow.Add("EnergyTotalReverseActivePower", 0);
            hashEntries = entries.ToArray();
            return datarow;
        }
    }
}
