using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.DataServices
{
    public abstract class MqttEventHandler : MQTTnet.Client.Disconnecting.IMqttClientDisconnectedHandler, IMqttClientConnectedHandler
    {
        

       

        public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class MqttFactoryHost : IDisposable
    {
        public MqttAddress Address { get; private set; }
        protected IMqttClient client;

        protected virtual void OnDisposing()
        { }

        public void Dispose()
        {
            if (client != null)
                client.Dispose();
            OnDisposing();
        }

        public async Task PublishAsync(string payload)
        {
            MqttApplicationMessage msg = CreateMqttMessage(payload);
            await this.client.PublishAsync(msg);
        }

        private MqttApplicationMessage CreateMqttMessage(string payload)
        {
            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(payload);
            var applicationMessage = new MqttApplicationMessageBuilder()
                       .WithTopic(Address.Topic)
                       .WithPayload(payload_buffer)
                       .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)Address.QosLevel)
                       .Build();
            return applicationMessage;
        }

        public MqttFactoryHost(MqttAddress mqttAddress)
        {
            Address = mqttAddress;

            //var ClientOptions = new MqttClientOptions
            //{
            //    ClientId = mqttOptions.ClientId,
            //    ChannelOptions = new MqttClientTcpOptions
            //    {
            //        Server = mqttOptions.BindAddress,
            //        Port = mqttOptions.Port
            //    },

            //};

            client = new MqttFactory().CreateMqttClient()
                .UseConnectedHandler(HandleConnectedAsync)
                .UseDisconnectedHandler(HandleDisconnectedAsync)
                .UseApplicationMessageReceivedHandler(HandleApplicationMessageReceivedEvent);

            Task<MqttClientAuthenticateResult> t = ConnectAsync();
            t.Wait();

            //{
            //    var result = await client.ConnectAsync(ClientOptions);
            //}
            //catch (Exception exception)
            //{
            //    _logger.LogError(exception, "### CONNECTING FAILED ###" + Environment.NewLine + exception);
            //}
        }

        public async Task<MqttClientAuthenticateResult> ConnectAsync()
        {
            return await client.ConnectAsync(GetMQttClientOptions());
        }

        private MqttClientOptions GetMQttClientOptions()
        {
            var ClientOptions = new MqttClientOptions
            {
                ClientId = Address.ClientId,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = Address.BindAddress,
                    Port = Address.Port
                },
            };
            return ClientOptions;
        }

        public virtual void OnInitialize() { }

        private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            await ConnectAsync();
            await OnDisconnected();
        }

        private async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            await OnConnected();
        }

        private Task HandleApplicationMessageReceivedEvent(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        protected virtual Task OnConnected()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDisconnected()
        {
            return Task.CompletedTask;
        }
    }
}
