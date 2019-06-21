using PEIU.MicroProcessor;
using System;

namespace EventReceiveMqtt
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ConfigBuilder builder = new ConfigBuilder();
            string conn = builder.GetConnectionString("mysql");
            MqttAddress[] addr = builder.GetValue<MqttAddress[]>("MQTTBrokers:EventBrokerAddress");
            
        }
    }

    
}
