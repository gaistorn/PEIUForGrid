using FireworksFramework.Mqtt;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PES.Service.DataService
{
    public class PeiuPublishWorker : AbsMqttPublisher
    {
        public int SiteId { get; set; }
        public string DeviceName { get; set; }
        private CancellationToken token;
        public PeiuPublishWorker(CancellationToken cancellationToken)
        {
            token = cancellationToken;
        }
        protected override string GetMqttPublishTopicName()
        {
            return "topic";
        }

        public async Task publish(int siteId, string DeviceId, string message)
        {
            string topicName = $"hubbub/{siteId}/{DeviceId}/AI";
            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(message);
            MqttApplicationMessage mqttMessage = new MqttApplicationMessageBuilder()
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)mqttPublishQos)
                .WithPayload(payload_buffer)
                .WithRetainFlag(mqttIsRetained)
                .WithTopic(topicName)
                .Build();


            try
            {
                var result = await mqttClient.PublishAsync(mqttMessage, token);
                logger.Debug("Message published. Message ID=" + result.PacketIdentifier);
                logger.Debug("Message: " + message);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
    }
}
