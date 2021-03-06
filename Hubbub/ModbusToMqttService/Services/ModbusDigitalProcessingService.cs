﻿using PEIU.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json.Linq;
using NHibernate;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PEIU.DataServices;
using NHibernate.Criterion;
using PEIU.Events;

namespace PEIU.Hubbub.Services
{
    public class ModbusDigitalProcessingService : BackgroundHostService
    {
        ILogger<ModbusBackgroundService> logger;
        IModbusFactory modbusFactory;
        IConfiguration config;
        ModbusSystem modbus;
        IDatabaseAsync redis;
        readonly EventPublisherWorker eventPublisherWorker;
        readonly EventMap event_map;
        short SiteId = -1;
        int DeviceIndex = 1;
        readonly TimeSpan UpdatePeriod = TimeSpan.FromSeconds(1);

        private readonly object locker = new object();
        IDataAccess dbAccess;
        public ModbusDigitalProcessingService(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            IDataAccess mysql_dataAccess, IRedisConnectionFactory redisFactory,
            IModbusFactory modbus_factory, IConfiguration configuration, EventPublisherWorker worker, EventMap map,
            ModbusSystem modbusSystem)
        {
            logger = loggerFactory.CreateLogger<ModbusBackgroundService>();
            config = configuration;
            modbusFactory = modbus_factory;
            eventPublisherWorker = worker;
            UpdatePeriod = configuration.GetSection("EventPollInterval").Get<TimeSpan>();
            modbus = modbusSystem;
            SiteId = configuration.GetSection("SiteId").Get<short>();
            redis = redisFactory.Connection().GetDatabase();
            DeviceIndex = configuration.GetSection("DeviceIndex").Get<int>();
            dbAccess = mysql_dataAccess;
            event_map = map;
        }

        
        private bool EqualValue(byte[] srcBitValue, ushort descValue)
        {
            ushort srcValue = BitConverter.ToUInt16(srcBitValue, 0);
            return srcValue == descValue;
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Completed");
            await Task.Delay(10);
        }

        //private async Task SendQueue(string topic, JObject msg_oridinary, CancellationToken token)
        //{
        //    foreach (var mqtt_proxy in mqtt_clients)
        //    {
        //        try
        //        {

        //            var msg = CreateMqttMessage(topic, msg_oridinary.ToString(), mqtt_proxy.Options.QosLevel);
        //            var mqtt_client = mqtt_proxy.MqttClient;

        //            if (mqtt_client.IsConnected == false)
        //                continue;
        //            await mqtt_client.PublishAsync(msg, token);
        //            Thread.Sleep(10);
        //            break;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError(ex, ex.Message);
        //        }
        //    }
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                stoppingToken.ThrowIfCancellationRequested();
                try
                {

                    bool isConnected = modbusFactory.ReconnectWhenDisconnected(stoppingToken, 3);
                    if (isConnected == false)
                    {
                        logger.LogWarning("모드버스 접속에 실패했습니다. 5초후 재개");
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }


                    
                    stoppingToken.ThrowIfCancellationRequested();
                    //var parallelResult = Parallel.ForEach<GroupPoint>(modbus.GroupPoints, (async x =>
                    foreach (var x in modbus.GroupDigitalPoints)
                    {
                        if (x.Disable == true)
                            continue;
                        List<DiMap> results = await modbusFactory.ReadModbusEvent(x);


                        foreach (DiMap map in results)
                        {
                            if (map.DocumentAddress == 40132)
                            {

                            }
                            if (map.Disable == true)
                                continue;



                            //cache.Get()
                            //datarow.Add(map.Name, map.Value);
                            string redis_key = $"{modbus.DeviceName}.{map.DocumentAddress}";
                            ushort redis_value = 0;
                            if (await redis.KeyExistsAsync(redis_key))
                            {
                                string redis_str = await redis.StringGetAsync(redis_key);
                                redis_value = ushort.Parse(redis_str);
                            }

                            if (map.Value == redis_value)
                                continue;
                            else
                            {
                                await redis.StringSetAsync(redis_key, map.Value.ToString());
                            }



                            using (var session = dbAccess.SessionFactory.OpenSession())
                            {
                                using (var transaction = session.BeginTransaction())
                                {
                                    EventSummary summary = new EventSummary();
                                    summary.SiteId = SiteId;
                                    summary.DeviceName = modbus.DeviceName;
                                    summary.GroupName = map.Name;

                                    summary.SetTimestamp(DateTime.Now);
                                    foreach (DiFlag evt in map.Flags)
                                    {
                                        EventField ef = event_map.FirstOrDefault(Tx => Tx.Register == map.DocumentAddress && Tx.BitValue == evt.BitValue);


                                        bool IsActive = evt.IsActivate(map.Value);
                                        if (evt.EventCode.HasValue)
                                            summary.ActiveEvents.Add(evt.EventCode.Value);
                                        string redis_event_key = redis_key + "." + evt.No;

                                        if (await redis.KeyExistsAsync(redis_event_key) == false)
                                        {
                                            await redis.HashSetAsync(redis_event_key, CreateEventNode(map, evt));
                                        }
                                        else
                                        {
                                            await redis.HashSetAsync(redis_event_key, "Status", IsActive);
                                        }

                                        if (map.Disable == true || map.Level == 0)
                                            continue;

                                        string mysql_key_str = $"{modbus.DeviceName}{map.DocumentAddress}{evt.No}";

                                        string normalizeDeviceName = map.Source + DeviceIndex;
                                        //ActiveEvent existEvent = session.Get<ActiveEvent>(mysql_key_str);
                                        ActiveEvent existEvent = await GetLatestActiveEventAsync(session, mysql_key_str, stoppingToken);
                                        //if (existEvent == null)
                                        //    continue;
                                        switch (EventStatus(existEvent, IsActive, session))
                                        {
                                            case Hubbub.EventStatus.Already:
                                                mysql_key_str = GetNextEventId(mysql_key_str, session);
                                                existEvent = new ActiveEvent();
                                                existEvent.Description = evt.BitName;
                                                existEvent.DeviceName = modbus.DeviceName;
                                                existEvent.EventLevel = map.Level;
                                                existEvent.EventName = evt.BitName;
                                                existEvent.Source = map.Source;
                                                existEvent.OccurTimestamp = DateTime.Now;
                                                existEvent.EventId = mysql_key_str;
                                                await session.SaveAsync(existEvent, stoppingToken);

                                                break;
                                            case Hubbub.EventStatus.New:
                                                existEvent = new ActiveEvent();
                                                existEvent.Description = evt.BitName;
                                                existEvent.DeviceName = modbus.DeviceName;
                                                existEvent.EventLevel = map.Level;
                                                existEvent.EventName = evt.BitName;
                                                existEvent.Source = map.Source;
                                                existEvent.OccurTimestamp = DateTime.Now;
                                                existEvent.EventId = mysql_key_str;
                                                await session.SaveAsync(existEvent, stoppingToken);
                                                if (evt.EventCode.HasValue)
                                                    summary.NewEvents.Add(evt.EventCode.Value);
                                                if (ef != null)
                                                    await eventPublisherWorker.PublishEvent(SiteId, normalizeDeviceName, ef.Code, Events.Alarm.EventStatus.New, stoppingToken);

                                                break;
                                            case Hubbub.EventStatus.Recover:
                                                if (evt.EventCode.HasValue)
                                                    summary.RecoverEvents.Add(evt.EventCode.Value);
                                                RecoversEvent(existEvent.EventId, session);
                                                if (ef != null)
                                                    await eventPublisherWorker.PublishEvent(SiteId, normalizeDeviceName, ef.Code, Events.Alarm.EventStatus.Recovery, stoppingToken);
                                                //existEvent.HasRecovered = true;
                                                //existEvent.RecoverTimestamp = DateTime.Now;
                                                //await session.SaveOrUpdateAsync(existEvent, stoppingToken);
                                                break;

                                        }
                                    }
                                    await transaction.CommitAsync(stoppingToken);

                                    //var msg = CreateMqttMessage(peiu_event_topic, summary.ToString(), 2);
                                    //await mqtt_clients.PeiuEventBrokerProxy.MqttClient.PublishAsync(msg, stoppingToken);
                                }
                            }
                        }


                    }
                }catch(Exception ex)
                {

                }
                
                //await SendQueue(peiu_event_topic, datarow, stoppingToken);
                await Task.Delay(UpdatePeriod.Milliseconds);
                //Thread.Sleep(UpdatePeriod);
            }

        }

        private async Task<ActiveEvent> GetLatestActiveEventAsync(ISession session, string eventId, CancellationToken token)
        {
            IList<ActiveEvent> ev = await session.CreateCriteria<ActiveEvent>().Add(Restrictions.Like("EventId", eventId + "%"))
                .AddOrder(NHibernate.Criterion.Order.Desc("EventId")).SetMaxResults(1).ListAsync<ActiveEvent>(token);

            return ev.FirstOrDefault();
                
            //SELECT* FROM grid.activeevent where EventId like 'JeJuGridPcs14018243%' order by EventId desc limit 1;
        }
        private async void RecoversEvent(string evtId, ISession session)
        {
            int num = 0;
            ActiveEvent newEvt = null;
            string newEvtId = null;
            while (true)
            {
                if(newEvtId == null)
                    newEvt = session.Get<ActiveEvent>(evtId);
                else
                    newEvt = session.Get<ActiveEvent>(newEvtId);

                if (newEvt == null)
                    break;
                newEvtId = $"{evtId}_{num++}";
                if (newEvt.HasRecovered) continue;
                newEvt.HasRecovered = true;
                newEvt.RecoverTimestamp = DateTime.Now;
                await session.SaveOrUpdateAsync(newEvt);
            }
        }

        private EventStatus EventStatus(ActiveEvent evt, bool IsActive, ISession session)
        {
            if (evt == null && IsActive)
                return Hubbub.EventStatus.New;
            else if (evt != null)
            {
                if(IsActive == false && evt.HasRecovered == false)
                {
                    return Hubbub.EventStatus.Recover;
                }
                else if(evt.HasRecovered && IsActive)
                {
                    return Hubbub.EventStatus.Already;
                }
            }
            return Hubbub.EventStatus.NoEvent;

        }

        private string GetNextEventId(string evtId, ISession session)
        {
            int num = 0;
            ActiveEvent newEvt = null;
            string newEvtId = null;
            while (true)
            {
                newEvtId = $"{evtId}_{num++}";
                newEvt = session.Get<ActiveEvent>(newEvtId);
                if (newEvt == null)
                    break;
            }
            return newEvtId;
        }

        private async Task<bool> EventTransitionAsync(CancellationToken token, ActiveEvent evt, ISession session)
        {

            bool HasCompleted = evt != null && evt.IsAck == true && evt.HasRecovered == true;
            if (HasCompleted == false)
                return false;
            LogEvent NewEvent = new LogEvent();
            NewEvent.EventId = evt.EventId;
            NewEvent.DeviceName = evt.DeviceName;
            NewEvent.EventName = evt.EventName;
            NewEvent.Description = evt.Description;
            NewEvent.OccurTimestamp = evt.OccurTimestamp;
            NewEvent.RecoverTimestamp = evt.RecoverTimestamp.HasValue ? evt.RecoverTimestamp.Value : DateTime.Now;
            NewEvent.ResolvedTimestamp = DateTime.Now;
            await session.SaveAsync(NewEvent, token);
            await session.DeleteAsync(NewEvent, token);
            return HasCompleted;
        }

        private HashEntry[] CreateEventNode(DiMap map, DiFlag flag)
        {
            HashEntry[] result = new HashEntry[]
            {
                new HashEntry("DocumentAddress", map.DocumentAddress),
                new HashEntry("EventName", map.Description),
                new HashEntry("IsEvent", map.Event),
                new HashEntry("Level", map.Level),
                new HashEntry("Description", flag.BitName),
                new HashEntry("BitValue", (int)flag.BitValue),
                new HashEntry("Status", flag.IsActivate(map.Value))
            };
            return result;
        }
    }
}
