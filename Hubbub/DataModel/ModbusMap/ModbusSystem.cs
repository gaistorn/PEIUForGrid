using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models {
    
    public class ModbusSystem {
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public int PortNum { get; set; }
        public byte SlaveId { get; set; }

        public  IEnumerable<GroupPoint> GroupPoints{ get; set; }
        public  IEnumerable<EventGroupPoint> GroupDigitalPoints { get; set; }
}
}
