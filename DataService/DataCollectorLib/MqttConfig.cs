using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.DataServices
{
    public class MqttConfig
    {
        public MqttAddress[] DataBrokerAddress { get; set; }

        public MqttAddress PEIUEventBrokerAddress { get; set; }
    }
}
