using DataModel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Hubbub.Services
{
    public class ModbusDigitalProcessingService : BackgroundService
    {
        ILogger<ModbusBackgroundService> logger;
        IModbusFactory modbusFactory;
        IConfiguration config;
        MqttConfig mqtt_config;
        IDatabaseAsync redis;
        private readonly object locker = new object();
        IDataAccess dbAccess;
        public ModbusDigitalProcessingService(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            IDataAccess mysql_dataAccess, IRedisConnectionFactory redisFactory,
            IModbusFactory modbus_factory, IConfiguration configuration)
        {
            logger = loggerFactory.CreateLogger<ModbusBackgroundService>();
            config = configuration;
            modbusFactory = modbus_factory;
            mqtt_config = configuration.GetSection("MQTTBrokers").Get<MqttConfig>();
            redis = redisFactory.Connection().GetDatabase();
            dbAccess = mysql_dataAccess;
        }

        
        private bool EqualValue(byte[] srcBitValue, ushort descValue)
        {
            ushort srcValue = BitConverter.ToUInt16(srcBitValue, 0);
            return srcValue == descValue;
        }

        private async Task ReadDigitalData(IEnumerable<PointList> readDiDatas, ModbusSystem system, CancellationToken token)
        {
            foreach (PointList map in readDiDatas)
            {
                if (map.Disable == 1)
                    continue;
                //cache.Get()
                string redis_key = $"{system.DeviceName}.DI.{map.GroupName}.{map.Address}";
                string topic = $"hubbub/{modbusFactory.SiteNo}/{system.DeviceName}/{map.GroupName}/AI";
                ushort redis_value = 0;
                if (await redis.KeyExistsAsync(redis_key))
                {
                    string redis_str = await redis.StringGetAsync(redis_key);
                    redis_value = ushort.Parse(redis_str);
                }
                ushort mapValue = Convert.ToUInt16(map.Value);
                if (mapValue == redis_value)
                    continue;
                else
                {
                    await redis.StringSetAsync(redis_key, mapValue.ToString());
                }



                using (var session = dbAccess.SessionFactory.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        foreach (DiFlag evt in map.Flags)
                        {
                            bool IsActive = evt.IsActivate(mapValue);
                            string redis_event_key = redis_key + "." + evt.No;

                            if (await redis.KeyExistsAsync(redis_event_key) == false)
                            {
                                await redis.HashSetAsync(redis_event_key, CreateEventNode(map, evt));
                            }
                            else
                            {
                                await redis.HashSetAsync(redis_event_key, "Status", IsActive);
                            }

                            if (map.Disable == 1 || map.Level == 0)
                                continue;

                            string mysql_key_str = $"{modbusFactory.SiteNo}{map.ID}{evt.No}";
                            int mysql_key_id = int.Parse(mysql_key_str);


                            ActiveEvent existEvent = session.Get<ActiveEvent>(mysql_key_id);
                            if (existEvent == null && IsActive)
                            {
                                existEvent = new ActiveEvent();
                                existEvent.DeviceUniqueId = system.DeviceId;
                                existEvent.Description = evt.BitName;
                                existEvent.DeviceName = system.DeviceName;
                                existEvent.EventLevel = map.Level;
                                existEvent.EventName = evt.BitName;
                                existEvent.OccurTimestamp = DateTime.Now;
                                existEvent.EventId = mysql_key_id;
                                await session.SaveAsync(existEvent, token);
                            }
                            else if (existEvent != null && IsActive == false) // 복구됨
                            {
                                existEvent.HasRecovered = true;
                                existEvent.RecoverTimestamp = DateTime.Now;
                                await session.SaveOrUpdateAsync(existEvent, token);
                            }

                            //await EventTransitionAsync(stoppingToken, existEvent, session);

                        }
                        await transaction.CommitAsync(token);
                    }
                }
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await modbusFactory.ReadDigitalData(ReadDigitalData, stoppingToken);
                await Task.Delay(10);

            }
        }

        private async Task<bool> EventTransitionAsync(CancellationToken token, ActiveEvent evt, ISession session)
        {

            bool HasCompleted = evt != null && evt.IsAck == true && evt.HasRecovered == true;
            if (HasCompleted == false)
                return false;
            LogEvent NewEvent = new LogEvent();
            NewEvent.EventId = evt.EventId;
            NewEvent.DeviceUniqueId = evt.DeviceUniqueId;
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

        private HashEntry[] CreateEventNode(PointList map, DiFlag flag)
        {
            HashEntry[] result = new HashEntry[]
            {
                new HashEntry("Address", (int)map.Address),
                new HashEntry("EventName", map.Description),
                new HashEntry("IsEvent", map.Level > 1),
                new HashEntry("Level", map.Level),
                new HashEntry("Description", flag.BitName),
                new HashEntry("BitValue", (int)flag.BitValue),
                new HashEntry("Status", flag.IsActivate((ushort)map.Value))
            };
            return result;
        }
    }
}
