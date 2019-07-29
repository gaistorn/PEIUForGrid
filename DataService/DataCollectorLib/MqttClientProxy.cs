using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PEIU.DataServices
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

    public class MqttClientProxyCollection : List<MqttClientProxy>
    {
        public MqttClientProxy PeiuEventBrokerProxy { get; set; }
    }
}
