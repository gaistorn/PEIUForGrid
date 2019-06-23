using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class ModbusSystem {
        public virtual int DeviceId { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual string IpAddress { get; set; }
        public virtual bool Disable { get; set; }
        public virtual int PortNum { get; set; }
        public virtual byte SlaveId { get; set; }
        //public virtual int ModbusFamilyId { get; set; }

        public virtual ModbusFamily ModbusFamily { get; set; }
    }
}
