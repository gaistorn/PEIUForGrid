using MQTTnet;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Criterion;
using PEIU.Events.Alarm;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.Events
{
    public class EventSubscribeWorker : FireworksFramework.Mqtt.AbsMqttSubscriberWorker<EventModel>
    {
        private ISessionFactory sessionFactory;
        readonly ConnectionMultiplexer redisConnection;
        readonly IDatabaseAsync redisDb;
        readonly EventPublisherWorker Publisher;
        public EventSubscribeWorker(ISessionFactory sessionFactory, ConnectionMultiplexer redisConnection)
        {
            this.sessionFactory = sessionFactory;
            this.redisConnection = redisConnection;
            redisDb = this.redisConnection.GetDatabase(1);
            Publisher = new EventPublisherWorker();
            Publisher.Initialize();
        }

        protected override async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            byte[] packet = e.ApplicationMessage.Payload;
            string txt = Encoding.UTF8.GetString(packet);
            try
            {
                EventModel data = JsonConvert.DeserializeObject<EventModel>(txt);
                using (var session = sessionFactory.OpenStatelessSession())
                using (var transaction = session.BeginTransaction())
                {
                    EventMap map = session.Get<EventMap>(data.EventCode);
                    string redisKey = $"SID{data.SiteId}.EVENT.{data.DeviceId}";
                    Program.logger.Info($"Received Event: {data.EventCode} / Status: {data.Status}");
                    switch (data.Status)
                    {
                        case EventStatus.New:
                            await redisDb.HashSetAsync(redisKey, $"{data.EventCode}", bool.TrueString);
                            // 새로 발생한 이벤트
                            IList<EventRecord> ev = await FindNewRaiseEvent(data.EventCode, session.CreateCriteria<EventRecord>());
                            if (ev.Count > 0)
                            {
                                return;
                            }
                            EventRecord newEventRecode = new EventRecord();
                            newEventRecode.CreateDT = new DateTime(1970, 1, 1).AddSeconds(data.UnixTimestamp).ToLocalTime();
                            newEventRecode.EventId = data.EventCode;
                            newEventRecode.SiteId = data.SiteId;
                            newEventRecode.DeviceId = data.DeviceId;
                            await session.InsertAsync(newEventRecode);
                            
                            break;
                        case EventStatus.Recovery:
                            await redisDb.HashSetAsync(redisKey, $"{data.EventCode}", bool.FalseString);
                            var rc_result = await FindNewRaiseEvent(data.EventCode, session.CreateCriteria<EventRecord>());
                            if (rc_result.Count > 0)
                            {
                                foreach (EventRecord exist_record in rc_result)
                                {
                                    exist_record.RecoveryDT = new DateTime(1970, 1, 1).AddSeconds(data.UnixTimestamp).ToLocalTime();
                                    await session.UpdateAsync(exist_record);
                                }
                            }
                            break;
                    }
                    await transaction.CommitAsync();


                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, ex.Message);
            }
        }

        private async Task<IList<EventRecord>> FindNewRaiseEvent(int eventcode, ICriteria criteria)
        {
            return await criteria.Add(Restrictions.Eq("EventId", eventcode))
                                .Add(Restrictions.IsNull("RecoveryDT")).ListAsync<EventRecord>();
        }
            
    }
}
