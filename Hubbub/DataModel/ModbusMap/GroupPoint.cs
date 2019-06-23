using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class GroupPoint {
        public virtual int GroupId { get; set; }
        public virtual string GroupName { get; set; }
        public virtual int PollIntervalSec { get; set; }
        public virtual int IoType { get; set; }
        public virtual int ModbusId { get; set; }
        public virtual byte SlaveId { get; set; }
        public virtual int RetryIntervalSec { get; set; }
        public virtual short Disable { get; set; }
        public virtual int? DeviceUniqueId { get; set; }
        public virtual IList<AiMap> AiMaps { get; set; }
    }
}