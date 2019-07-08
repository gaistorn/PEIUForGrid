using DataModel;
using Microsoft.Extensions.Hosting;
using PEIU.Hubbub.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Hubbub.Services
{
#if CONTROL_TEST
    public class ControlLogService : BackgroundService
    {
        readonly StreamWriter loggerWriter;
        readonly Random seed = new Random();

        public ControlLogService(ModbusSystem modbusSystem)
        {
            loggerWriter = new StreamWriter(".\\ControlLog" + modbusSystem.SlaveId + ".log", true);
            loggerWriter.AutoFlush = true;

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            float lastcommand = DeviceController.LastControl;
            DateTime nextUpdateCommand = DateTime.MinValue;
            bool Changing = false;
            
            while (!stoppingToken.IsCancellationRequested)
            {
                if (DeviceController.TestMode == false)
                {
                    await Task.Delay(20);
                    continue;
                }
                if (ModbusBackgroundService.Soc == -1)
                    continue;
                if(lastcommand != DeviceController.LastControl && Changing == false)
                {
                    int ms = seed.Next(100, 200);
                    nextUpdateCommand = DateTime.Now.AddMilliseconds(ms);
                    Changing = true;
                }
                if(nextUpdateCommand < DateTime.Now)
                {
                    lastcommand = DeviceController.LastControl;
                    Changing = false;
                }
                loggerWriter.WriteLine("{0},{1},{2},{3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), ModbusBackgroundService.Soc, lastcommand, DeviceController.LastControl);
                
                await Task.Delay(20);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            loggerWriter.Close();
            return base.StopAsync(cancellationToken);
        }
    }
#endif
}
