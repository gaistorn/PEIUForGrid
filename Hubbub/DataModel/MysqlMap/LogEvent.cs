using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class LogEvent {
        public virtual long LogEventId { get; set; }
        public virtual int EventId { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual string EventName { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime OccurTimestamp { get; set; }
        public virtual int EventLevel { get; set; }
        public virtual DateTime AckTimestamp { get; set; }
        public virtual DateTime ResolvedTimestamp { get; set; }
        public virtual DateTime RecoverTimestamp { get; set; }
        public virtual int? DeviceUniqueId { get; set; }
    }
}
