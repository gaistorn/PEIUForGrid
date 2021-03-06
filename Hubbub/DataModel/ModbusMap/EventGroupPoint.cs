using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models {
    
    public class EventGroupPoint {
        public virtual int GroupId { get; set; }
        public virtual string GroupName { get; set; }
        public virtual int? PollIntervalMs { get; set; }
        public virtual bool? Disable { get; set; }
        public virtual ushort StartAddress { get; set; }
        public virtual IList<DiMap> DigitalPoints { get; set; }

        
    }
}
