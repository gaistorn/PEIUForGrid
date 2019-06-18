using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class PcsEvent : DataObject
    {
        public virtual Deviceinfo Deviceinfo { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual string Message { get; set; }
        public virtual string Isack { get; set; }
        public virtual DateTime? Acktimestamp { get; set; }
        public virtual string Ackuser { get; set; }
        public virtual string Category { get; set; }
        public virtual string Isfault { get; set; }
    }
}
