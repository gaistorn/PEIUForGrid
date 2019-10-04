using FireworksFramework.Mqtt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using PEIU.Models;
using Microsoft.Extensions.Logging;

namespace EtriCommandAgent
{
    public class EtriCommandPublisher : AbsMqttPublisher
    {
        public int DeviceIndex { get; set; }
        private readonly ILogger _logger;

        public EtriCommandPublisher(ILogger<EtriCommandPublisher> logger)
        {
            this._logger = logger;
            this.Initialize();
        }

        public async Task PublishAsync(CancellationToken token, int PcsNo, ushort Address, params ushort[] values)
        {
            ModbusControlModel model = new ModbusControlModel();
            model.StartAddress = Address;
            model.WriteValues = values;
            string message = JsonConvert.SerializeObject(model);
            this.DeviceIndex = PcsNo;
            _logger.LogInformation($"[제어명령] TopicName:{GetMqttPublishTopicName()} PCS 대상: {PcsNo} 제어주소: {Address} 명령값: {string.Join(" ", values)}");
            await base.PublishMessageAsync(message, token);
        }
        

        protected override string GetMqttPublishTopicName()
        {
            return $"hubbub/6/PCS{DeviceIndex}/Control";
        }
    }
}
