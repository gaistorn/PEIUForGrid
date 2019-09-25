using FireworksFramework.Mqtt;
using PEIU.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Event
{
    class Program
    {
        static  void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            EventPublisherWorker worker = new EventPublisherWorker();
            worker.Initialize();
            CancellationTokenSource tk = new CancellationTokenSource();
            while (true)
            {
                Console.Write("Input the Event Code: ");
                string code = Console.ReadLine();
                if(int.TryParse(code, out int EventCode ))
                {
                    Task t = worker.PublishEvent(6, "PCS1", EventCode, Events.Alarm.EventStatus.New, tk.Token);
                    t.Wait();
                }
            }
        }
    }
}
