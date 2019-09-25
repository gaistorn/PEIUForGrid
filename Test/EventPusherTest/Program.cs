using PEIU.DataServices;
using PEIU.Hubbub;
using PEIU.MicroProcessor;
using PEIU.Models;
using System;

namespace EventPusherTest
{
    class Program
    {
        static void Main(string[] args)
        {
            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            DateTime dt = new DateTime(1970, 1, 1).AddSeconds(unix);

            DateTime newdt = dt.ToLocalTime();
            Console.WriteLine("Hello World!");

            MqttAddress address = new MqttAddress();
            address.BindAddress = "www.peiu.co.kr";
            address.Port = 2084;
            address.QosLevel = 2;
            
                 string peiu_event_topic = $"hubbub/{6}/Event";
            address.Topic = peiu_event_topic;
            using (EventPusher pusher = new EventPusher(address, true))
            {
                for(int i=0;i<8;i++)
                {
                    double pow = Math.Pow(2, i);
                    pusher.AddEvent("PCS_FAULT1", (ushort)(100 + i), (ushort)pow);
                }
                
                

                while (true)
                {
                    Console.Write("Input Value: ");
                    string str = Console.ReadLine();
                    if (str.ToUpper() == "QUIT")
                        break;
                    ushort iValue;
                    
                    if (ushort.TryParse(str, out iValue))
                    {
                        pusher.BeginProcessing();
                        EventSummary summary = null;
                        bool isEvent = pusher.ProcessingEvent(6, "Jeju1", "PCS_FAULT1", iValue, ref summary);
                        if (isEvent)
                        {
                            pusher.EventPushing(summary);
                            Console.WriteLine(summary);
                        }
                        pusher.EndProcessing();
                    }
                    
                }
               
            }

            
        }
    }
}
