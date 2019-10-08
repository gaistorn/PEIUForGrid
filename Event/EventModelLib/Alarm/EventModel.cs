using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Events.Alarm
{
    public class EventModel
    {
        public int SiteId { get; set; }
        public string DeviceId { get; set; }
        public long UnixTimestamp { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventStatus Status { get; set; }

        public DiMap Detail { get; set; }
        public DateTime TimeStamp
        {
            get
            {
                return new DateTime(1970, 1, 1).AddSeconds(UnixTimestamp).ToLocalTime();
            }
        }

    }

    public enum EventStatus
    {
        New,
        Recovery
    }
}
