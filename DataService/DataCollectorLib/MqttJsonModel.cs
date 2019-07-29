using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DataModel
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
        public int QosLevel { get; set; }

        public string ToJson()
        {
            string str = JsonConvert.SerializeObject(this);
            return str;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class MqttConfig
    {
        [JsonProperty("DataBrokerAddress")]
        public MqttAddress[] DataBrokerAddress { get; set; }

        public MqttAddress PEIUEventBrokerAddress { get; set; }
    }
}
