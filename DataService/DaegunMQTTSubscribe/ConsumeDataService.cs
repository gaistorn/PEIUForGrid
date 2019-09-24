using PEIU.Hubbub;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PES.Service.DataService
{
    public class ConsumeDataService : BackgroundHostService
    {
        private PeiuPublishWorker PeiuPublishWorker;
        private IPacketQueue queue;

        public ConsumeDataService(IPacketQueue packetQueue)
        {
            queue = packetQueue;
            
        }

        private async Task PublishAsync(DaegunPacket packet)
        {
            string pcs_string = null;
            string bms_string = null;
            string pv_string = null;
            if (MqttModelConvert.ConvertPcs(packet, DateTime.Now, ref pcs_string, ref bms_string, ref pv_string))
            {
                //await base.p
                await PeiuPublishWorker.publish(packet.sSiteId, "PCS" + packet.Pcs.PcsNumber, pcs_string);
                await PeiuPublishWorker.publish(packet.sSiteId, "BMS" + packet.Bsc.PcsIndex, bms_string);
                await PeiuPublishWorker.publish(packet.sSiteId, "PV" + packet.Pv.DeviceIndex, pv_string);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            PeiuPublishWorker = new PeiuPublishWorker(stoppingToken);
            PeiuPublishWorker.Initialize();
            while(true)
            {
                stoppingToken.ThrowIfCancellationRequested();
                if (stoppingToken.IsCancellationRequested)
                    break;
                DaegunPacket packet = await queue.DequeueAsync(stoppingToken);
                await PublishAsync(packet);
            }
        }
    }
}
