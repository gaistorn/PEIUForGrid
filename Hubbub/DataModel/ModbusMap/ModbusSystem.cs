using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class ModbusSystem {
        public virtual int Id { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual string IpAddress { get; set; }
        public virtual bool Disable { get; set; }
        public virtual int PortNum { get; set; }

        public virtual IList<GroupPoint> GroupPoints{ get; set; }

        public virtual IList<EventGroupPoint> GroupDigitalPoints { get; set; }
}
}
