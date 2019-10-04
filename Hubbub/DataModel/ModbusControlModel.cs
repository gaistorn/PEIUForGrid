using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class ModbusControlModel
    {
        public ushort StartAddress { get; set; }
        public ushort[] WriteValues { get; set; }
    }

   
}
