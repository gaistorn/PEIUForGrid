using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class DeviceEvent
    {
        public virtual string EventId { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual int EventCode { get; set; }
        public virtual DateTime OccurTimestamp { get; set; }
        public virtual bool IsAck { get; set; }
        public virtual DateTime? AckTimestamp { get; set; }
        public virtual bool HasRecovered { get; set; }
        public virtual DateTime? RecoverTimestamp { get; set; }
        public virtual int? siteId { get; set; }
    }
}
