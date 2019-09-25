using FireworksFramework.Mqtt;
using Newtonsoft.Json;
using PEIU.Events.Alarm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Events
{
    public class EventPublisherWorker : AbsMqttPublisher
    {
        public int SiteId { get; set; } = -1;
        public string DeviceId { get; set; }

        protected override string GetMqttPublishTopicName()
        {
            //if (string.IsNullOrEmpty(DeviceId) || SiteId == -1)
            //    throw new Exception("DeviceId 또는 SiteId가 설정되지 않았습니다");
            return $"hubbub/{SiteId}/{DeviceId}/Event";
        }

        public Task PublishEvent(EventMap map, int SiteId, string DeviceId, int EventCode, EventStatus status, CancellationToken token)
        {
            EventMqttModel record = new EventMqttModel();
            record.UnixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            record.DeviceId = this.DeviceId = DeviceId;
            record.SiteId = this.SiteId = SiteId;
            record.EventCode = EventCode;
            record.Status = status;
            record.Detail = map;
            string message = JsonConvert.SerializeObject(record);
            return base.PublishMessageAsync(message, token);
        }
    }
}
