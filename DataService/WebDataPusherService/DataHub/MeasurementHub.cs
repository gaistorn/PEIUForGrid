using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDataPusherService.DataHub
{
    public interface IClientMeasurementContract
    {
        Task ReceivePcsStock(int siteId, string DeviceId, JObject stock);
        Task ReceiveBatStock(int siteId, string DeviceId, JObject stock);
        Task ReceiveBatInfoStock(int siteId, string DeviceId, JObject stock);
        Task ReceivePvStock(int siteId, string DeviceId, JObject stock);
        Task ReceiveEventStock(int siteId, string DeviceId, JObject stock);
    }

    //public interface IServerMeasurementContract
    //{
    //    Task SendPCSData(int siteId, string deviceId, JObject obj);
    //    Task SendBMSData(int siteId, string deviceId, JObject obj);
    //    Task SendPVData(int siteId, string deviceId, JObject obj);
    //}

    public class MeasurementHub : Hub<IClientMeasurementContract>
    {
        ILogger<MeasurementHub> logger;
        public MeasurementHub(ILogger<MeasurementHub> _logger)
        {
            logger = _logger;
        }

        

        public override Task OnConnectedAsync()
        {
            logger.LogInformation("Connected Measurement Hub");
            return base.OnConnectedAsync();
        }


        //public async Task SendPCSData(int siteId, string deviceId, JObject obj)
        //{
        //    this.Clients.All
        //    await Clients.All.SendAsync("ReceivePcsStock", siteId, deviceId, obj);
        //}

        //public async Task SendBMSData(int siteId, string deviceId, JObject obj)
        //{
        //    await Clients.All.SendAsync("ReceiveBatStock", siteId, deviceId, obj);
        //}

        //public async Task SendPVData(int siteId, string deviceId, JObject obj)
        //{
        //    await Clients.All.SendAsync("ReceivePVStock", siteId, deviceId, obj);
        //}
    }
}
