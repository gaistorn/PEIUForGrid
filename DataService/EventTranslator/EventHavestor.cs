using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PEIU.DataServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.Core
{
    public class EventHavestor : DataServices.DataSubscribeWorker
    {
        readonly IBackgroundTaskQueue<EventSummary> _queue;
        public EventHavestor(IBackgroundTaskQueue<EventSummary> queue)
        {
            _queue = queue;
        }
        protected override Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            try
            {
                string msg = Encoding.UTF8.GetString(payload);
                JObject json_obj = JObject.Parse(msg);
                
                EventSummary summary = JsonConvert.DeserializeObject<EventSummary>(msg);
                _queue.QueueWorkItem(summary);
            }
            finally
            {
                
            }
            return Task.CompletedTask;

        }
    }
}
