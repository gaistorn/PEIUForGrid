using PEIU.Models;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PEIU.DataServices;

namespace PVMeasure
{
    public class MqttClientProxy
    {
        public IMqttClient MqttClient { get; private set; }
        public MqttAddress Options { get; private set; }

        public MqttClientProxy(IMqttClient client, MqttAddress options)
        {
            MqttClient = client;
            Options = options;
        }
    }

    //public class MqttClientProxyCollection : List<MqttClientProxy>
    //{

    //}

    public class MqttClientProxyCollection : List<MqttClientProxy>
    {
        public MqttClientProxy PeiuEventBrokerProxy { get; set; }
    }
}
