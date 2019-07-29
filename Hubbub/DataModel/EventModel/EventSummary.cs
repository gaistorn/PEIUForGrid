using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class EventSummary
    {
        public string DeviceName { get; set; }
        public short SiteId { get; set; }
        public string GroupName { get; set; }
        public string Timestamp { get; set; }
        
        public string EventId
        {
            get
            {
                return $"{SiteId}.{DeviceName}.{GroupName}";
            }
        }

        public const string DateTimeFormat = "yyyyMMddHHmmss";

        public void SetTimestamp(DateTime time)
        {
            Timestamp = time.ToString(DateTimeFormat);
        }

        public  DateTime GetTimestamp()
        {
            if (string.IsNullOrEmpty(Timestamp))
                return default(DateTime);
            else
            {
                return DateTime.ParseExact(Timestamp, DateTimeFormat, null);
            }
        }

        public List<ushort> NewEvents { get; } = new List<ushort>();
        public List<ushort> RecoverEvents { get; } = new List<ushort>();
        public List<ushort> ActiveEvents { get; } = new List<ushort>();
        public bool HasOccurEvent
        {
            get
            {
                return NewEvents.Count > 0 || RecoverEvents.Count > 0;
            }
        }

        public override string ToString()
        {
            return CreateJObject().ToString();
        }

        public JObject CreateJObject()
        {
            JObject evtRow = JObject.FromObject(this);
            return evtRow;
        }

    }
}
