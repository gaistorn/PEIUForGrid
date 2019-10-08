using FireworksFramework.Mqtt;
using Newtonsoft.Json;
using PEIU.Events.Alarm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Events
{
    public class EventPublisherWorker : AbsMqttPublisher
    {
        public int SiteId { get;  } = -1;
        public int FactoryCode { get;  }
        public string DeviceId { get; set; }

        private IEnumerable<DiMap> diMaps;

        public EventPublisherWorker(int siteId, int FactoryCode, IEnumerable<DiMap> MapList)
        {
            SiteId = siteId;
            this.FactoryCode = FactoryCode;
            this.diMaps = MapList;
        }

        protected override string GetMqttPublishTopicName()
        {
            //if (string.IsNullOrEmpty(DeviceId) || SiteId == -1)
            //    throw new Exception("DeviceId 또는 SiteId가 설정되지 않았습니다");
            return $"hubbub/{SiteId}/{DeviceId}/Event";
        }

        public Task PublishEvent(string DeviceId, int GroupCode, ushort BitValue, EventStatus status, CancellationToken token)
        {
            DiMap target_map = diMaps.FirstOrDefault(x=>x.)
            EventModel record = new EventModel();
            record.UnixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            record.DeviceId = this.DeviceId = DeviceId;
            record.SiteId =  SiteId;
            record.Status = status;
            record.Detail = map;
            string message = JsonConvert.SerializeObject(record);
            return base.PublishMessageAsync(message, token);
        }
    }
}
