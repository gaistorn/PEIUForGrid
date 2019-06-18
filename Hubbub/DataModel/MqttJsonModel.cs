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
    public class MqttJsonModel
    {
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("registers")]
        public MqttRegisterJsonModel[] Registers { get; set; }

        public string ToJson()
        {
            string str = JsonConvert.SerializeObject(this);
            return str;
        }

        public string ToRegisterArrayJson()
        {
            JObject obj = new JObject();
            JArray jArray = JArray.FromObject(Registers);
            
            obj.Add("registers", jArray);
            string str = obj.ToString();//JsonConvert.SerializeObject(obj);
            return str;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class MqttRegisterJsonModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("adddress")]
        public ushort Adddress { get; set; }

        [JsonProperty("length")]
        public byte Length { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public modbus_type Type { get; set; }

        [JsonProperty(PropertyName = "scale")]
        public float Scale { get; set; } = 1;

        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime timestamp { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class MqttInlineRegisterJsonModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("adddress")]
        public ushort Adddress { get; set; }

        [JsonProperty("length")]
        public byte Length { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public modbus_type Type { get; set; }

        [JsonProperty(PropertyName = "scale")]
        public float Scale { get; set; } = 1;

        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        [JsonProperty(PropertyName = "lastupdate")]
        public DateTime Lastupdate { get; set; }

        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }

        public string ToJson()
        {
            string str = JsonConvert.SerializeObject(this);
            return str;
        }
    }

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
    }
}
