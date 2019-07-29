using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PEIU.DataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDataPusherService.DataHub;

namespace WebDataPusherService
{
    public class MqttSubscribeWorker : DataSubscribeWorker, IHostedService
    {
        readonly MqttAddress mqttAddress;
        readonly IHubContext<MeasurementHub, IClientMeasurementContract> hubContext;
        readonly ILogger<MqttSubscribeWorker> logger;
        const string PCS_SYSTEM = "PCS_SYSTEM";
        const string PCS_BMSINFO = "PCS_BMSINFO";
        const string PCS_BATTERY = "PCS_BATTERY";
        const string PV_SYSTEM = "PV_SYSTEM";

        public MqttSubscribeWorker(ILogger<MqttSubscribeWorker> _logger, MqttAddress _address, IHubContext<MeasurementHub, IClientMeasurementContract> _hubContext)
        {
            logger = _logger;
            mqttAddress = _address;
            hubContext = _hubContext;
        }

        protected override async Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            try
            {
                string data = Encoding.UTF8.GetString(payload);
                JObject jObj = JObject.Parse(data);
                string groupName = jObj["groupname"].Value<string>();
                int siteId = jObj["siteId"].Value<int>();
                string deviceId = jObj["deviceId"].Value<string>();
                if(groupName.Equals(PCS_SYSTEM))
                {
                    await hubContext.Clients.All.ReceivePcsStock(siteId, deviceId, jObj);
                   // await hubContext.Clients.All.
                }
                else if(groupName.Equals(PCS_BATTERY))
                {
                    await hubContext.Clients.All.ReceiveBatStock(siteId, deviceId, jObj);
                }
                else if(groupName.Equals(PCS_BMSINFO))
                {
                    await hubContext.Clients.All.ReceiveBatInfoStock(siteId, deviceId, jObj);
                }
                else if(groupName.Equals(PV_SYSTEM))
                {
                    await hubContext.Clients.All.ReceivePvStock(siteId, deviceId, jObj);
                }
                else if(groupName.Equals("EVENT"))
                {
                    await hubContext.Clients.All.ReceiveEventStock(siteId, deviceId, jObj);
                }

            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Method: OnApplicationMessageReceived\n" + ex.Message);
            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.ConnectionAsync(mqttAddress.ClientId, mqttAddress.BindAddress, mqttAddress.Port, mqttAddress.QosLevel, mqttAddress.Topic);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }
    }
}
