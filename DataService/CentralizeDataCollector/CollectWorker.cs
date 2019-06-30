using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.DataServices
{
    public class CollectWorker : DataSubscribeWorker
    {
        readonly IBackgroundTaskQueue<JObject> _queue = null;

        public Dictionary<string, string> ColumnConvertMap { get; } = new Dictionary<string, string>()
        {
            {"timestamp", "updateTime" },
            {"groupid", null },
            {"groupname", null }
        };

        public CollectWorker(IBackgroundTaskQueue<JObject> queue)
        {
            _queue = queue;
        }

        protected override Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            string payload_string = Encoding.UTF8.GetString(payload);
            JObject obj = JObject.Parse(payload_string);
            
            _queue.QueueWorkItem(obj);
            return Task.CompletedTask;
        }
    }
}
