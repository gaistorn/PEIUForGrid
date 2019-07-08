using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            StreamWriter sw = new StreamWriter(".\\log.log", true);

            DateTime endTime = DateTime.Now.AddMinutes(1);
            DateTime logTime = DateTime.MinValue;
            sw.AutoFlush = true;
            while (DateTime.Now < endTime)
            {
                sw.WriteLine(DateTime.Now);
                if(logTime < DateTime.Now)
                {
                    logTime = DateTime.Now.AddSeconds(5);
                    
                }
            }

            sw.Close();
        }
    }
}
