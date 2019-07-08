using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttProxy
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MqttAddress
    {
        //   "BindAddress": "192.168.0.5",
        //"Port": 1883,
        //"QosLevel": 2
        [JsonProperty("ClientId")]
        public string ClientId { get; set; }

        [JsonProperty("BindAddress")]
        public string BindAddress { get; set; }

        [JsonProperty("Port")]
        public ushort Port { get; set; } = 1883;

        [JsonProperty("QosLevel")]
        public ushort QosLevel { get; set; }

        public string Topic { get; set; }

        public string ToJson()
        {
            string str = JsonConvert.SerializeObject(this);
            return str;
        }
    }
}
