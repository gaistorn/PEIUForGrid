using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class vw_ActiveEvent
    {
        public virtual string EventId { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual int EventCode { get; set; }
        public virtual string EventName { get; set; }
        public virtual string Description { get; set; }
        public virtual string Category { get; set; }
        public virtual string Source { get; set; }
        public virtual short EventLevel { get; set; }
        public virtual short EventType { get; set; }
        public virtual DateTime OccurTimestamp { get; set; }
        public virtual bool IsAck { get; set; }
        public virtual DateTime? AckTimestamp { get; set; }
        public virtual bool HasRecovered { get; set; }
        public virtual DateTime? RecoverTimestamp { get; set; }
        public virtual int? SiteId { get; set; }
    }
}
