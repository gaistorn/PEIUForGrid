using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;
using PEIU.DataServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttProxy
{
    public class SubscribeWorker : DataSubscribeWorker
    {
        private IMqttClient _client;
        NLog.ILogger logger;
        MqttAddress to_address;
        public SubscribeWorker(NLog.ILogger _logger, string topic, MqttAddress to)
        {
            logger = _logger;
            to_address = to;
            _client = CreateMqttClient(to);
        }



        protected override async Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            
            string json_str = Encoding.UTF8.GetString(payload);
            JObject obj = JObject.Parse(json_str);
            int groupid = obj["groupid"].Value<int>();
            if (groupid == 1 || groupid == 2)
                logger.Trace($"RECEIVED Local Broker({obj["deviceId"]}.{obj["groupname"]}) from {Topic}. {obj["timestamp"]}");
            
            var msg = CreateMqttMessage(Topic, payload, QosLevel);
            if (_client.IsConnected == false)
                await _client.ReconnectAsync();
            //if(_client.IsConnected == false && groupid == 1 || groupid == 2)
            //{
            //    logger.Trace($"DISCONNECTED REMOTE BROKER (ClientId: {_client.Options.ClientId}");
            //    return;
            //}
            var result = await _client.PublishAsync(msg);
            
            if (groupid == 1 || groupid == 2)
            {
                logger.Trace($"SENDING REMOTE BROKER. Result:{Enum.GetName(typeof(MQTTnet.Client.Publishing.MqttClientPublishReasonCode), result.ReasonCode)}");
            }
            await Task.CompletedTask;
        }

        private MqttApplicationMessage CreateMqttMessage(string topic, byte[] payload, uint qos)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                       .WithTopic(topic)
                       .WithPayload(payload)
                       .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
                       .Build();
            return applicationMessage;
        }

        private  IMqttClient CreateMqttClient(MqttAddress addr)
        {
            var ClientOptions = new MqttClientOptions
            {
                ClientId = addr.ClientId,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = addr.BindAddress,
                    Port = addr.Port,

                }

            };

            var mqttClient = new MqttFactory().CreateMqttClient();

            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
            {
                //Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(1));

                try
                {
                    await mqttClient.ReconnectAsync();
                    logger.Trace("RECONNECTED BROKER");
                }
                catch
                {
                    logger.Trace("### RECONNECTING FAILED ###");
                }
            });
            bool IsSuccess = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    mqttClient.ConnectAsync(ClientOptions);
                    IsSuccess = true;
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"#### MQTT BROKER CONNECTING FAILED ### \n{addr.ToJson()}");
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    continue;
                }
            }

            if (IsSuccess == false)
            {
                logger.Error("#### 브로커 접속에 실패했습니다. 다시 실행해주세요. ####");
                return null;
            }
            return mqttClient;
        }
    }
}
