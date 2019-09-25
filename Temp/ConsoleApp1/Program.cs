using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static ConcurrentBag<string> vs;
        static readonly object lockObj = new object();
        static ManualResetEvent mre = new ManualResetEvent(true);
        static void Main(string[] args)
        {
            
            //string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //StreamWriter sw = new StreamWriter(".\\log.log", true);

            //DateTime endTime = DateTime.Now.AddMinutes(1);
            //DateTime logTime = DateTime.MinValue;
            //sw.AutoFlush = true;
            //while (DateTime.Now < endTime)
            //{
            //    sw.WriteLine(DateTime.Now);
            //    if(logTime < DateTime.Now)
            //    {
            //        logTime = DateTime.Now.AddSeconds(5);

            //    }
            //}

            //sw.Close();
            vs = new ConcurrentBag<string>();
            Task t = Task.Factory.StartNew(Publish);
            //Task t1 = Task.Factory.StartNew(ConsumeList);

            t.ContinueWith(x => ConsumeList());
            Console.WriteLine("Wait..");
            Console.ReadKey();
        }


        
        static async void Publish()
        {
            int idx = 0;
            
            while(true)
            {
                mre.WaitOne();
                string name = $"Test_{idx++}";
                Console.WriteLine("INSERT " + name);
                vs.Add(name);
                await Task.Delay(200);
            }
        }

        static async Task ConsumeList()
        {
            DateTime dt = DateTime.MinValue;
            while (true)
            {
                if(DateTime.Now >= dt)
                {
                    mre.Reset();
                    while (vs.IsEmpty == false)
                    {
                        string result = null;
                        if (vs.TryTake(out result))
                        {
                            Console.WriteLine(result + " CONSUME");
                            Thread.Sleep(500);
                        }
                        
                    }
                    Console.WriteLine("CLEAR");
                    dt = DateTime.Now.AddSeconds(5);
                    mre.Set();
                }
                
            }
        }
    }
}
