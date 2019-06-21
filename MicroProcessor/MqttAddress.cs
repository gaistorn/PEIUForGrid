using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.MicroProcessor
{
    public sealed class MqttAddress
    {
        //   "BindAddress": "192.168.0.5",
        //"Port": 1883,
        //"QosLevel": 2
        public string ClientId { get; set; }

        public string BindAddress { get; set; }

        public ushort Port { get; set; } = 1883;

        public int QosLevel { get; set; }

        public string Topic { get; set; }

    }
}
