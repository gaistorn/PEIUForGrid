using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayDeviceFEP
{
    public class ModbusConfig
    {
        public string SerialPort { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int BaudRate { get; set; } = 19200;
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(10);

        [JsonConverter(typeof(StringEnumConverter))]
        public System.IO.Ports.Parity Parity { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public System.IO.Ports.StopBits StopBits { get; set; }

        public int StartAddress { get; set; }
    }
}
