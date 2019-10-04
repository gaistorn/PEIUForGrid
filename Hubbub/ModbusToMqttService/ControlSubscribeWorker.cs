using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using PEIU.Events.Alarm;
using PEIU.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Hubbub
{
    public class ControlSubscribeWorker : FireworksFramework.Mqtt.AbsMqttSubscriberWorker<EventModel>, IHostedService
    {
        readonly ConnectionMultiplexer redisConnection;
        readonly IDatabaseAsync redisDb;
        readonly IConfiguration config;
        readonly IModbusFactory modbusFactory;
        readonly int SiteId;
        readonly int DeviceIndex;
        readonly ModbusSystem modbus;
        readonly CancellationToken CancelToken;
        public ControlSubscribeWorker(IModbusFactory modbus_factory,
            IRedisConnectionFactory redisFactory,
            IConfiguration configuration, ModbusSystem modbusSystem)
        {
            config = configuration;
            modbusFactory = modbus_factory;
            SiteId = configuration.GetSection("SiteId").Get<int>();
            DeviceIndex = configuration.GetSection("DeviceIndex").Get<int>();
            modbus = modbusSystem;
            redisDb = redisFactory.Connection().GetDatabase(1);
            CancelToken = Program.CancellationTokenSource.Token;
            this.Initialize();
        }

        protected override async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            byte[] packet = e.ApplicationMessage.Payload;
            string txt = Encoding.UTF8.GetString(packet);
            try
            {
                ModbusControlModel data = JsonConvert.DeserializeObject<ModbusControlModel>(txt);
                bool IsSuccess = await modbusFactory.WriteMultipleRegistersAsync(CancelToken, data.StartAddress, data.WriteValues);
                if (IsSuccess)
                    logger.Info($"Control Success -> {txt}");
                else
                    logger.Warn($"Control Failed -> {txt}");

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                CancelToken.ThrowIfCancellationRequested();
                try
                {
                    await base.MqttSubscribeAsync(CancelToken);
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                }

            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
