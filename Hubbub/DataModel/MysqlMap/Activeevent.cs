using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models {

    public class ActiveEvent
    {
        public virtual string EventId { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual string EventName { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime OccurTimestamp { get; set; }
        public virtual bool IsAck { get; set; }
        public virtual int EventLevel { get; set; }
        public virtual DateTime? AckTimestamp { get; set; }
        public virtual bool HasRecovered { get; set; }
        public virtual DateTime? RecoverTimestamp { get; set; }
        public virtual string Source { get; set; }
        public virtual short? siteId { get; set; }
    }

    public class NotifyEvent
    {
        public int EventCode { get; set; }
        public string DeviceName { get; set; }
        public short SiteId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public EventTriggerType TriggerType { get; set; }
        public DateTime Timestamp { get; set; }
        public int EventLevel { get; set; }
    }
}
