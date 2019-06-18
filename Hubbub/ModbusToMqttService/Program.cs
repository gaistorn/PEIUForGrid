using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PEIU.Hubbub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Assembly asm = Assembly.LoadFile(@"D:\MyProject\ModbusToMqtt\ModbusToMqttService\DataModel\bin\Debug\netcoreapp2.1\DataModel.dll");
            //Assembly dataModelAssembly = Assembly.Load("DataModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            //object deviceInfo = dataModelAssembly.CreateInstance("DataModel.PcsSystem");
            //if(deviceInfo != null)
            //{

            //}
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
