using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.MicroProcessor
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

    public abstract class MqttFactoryUtil
    {
        public MqttAddress Address { get; private set; }
        private IMqttClient client;

        public MqttFactoryUtil(MqttAddress mqttAddress)
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

            ConnectAsync();

            //{
            //    var result = await client.ConnectAsync(ClientOptions);
            //}
            //catch (Exception exception)
            //{
            //    _logger.LogError(exception, "### CONNECTING FAILED ###" + Environment.NewLine + exception);
            //}
        }

        public async void ConnectAsync()
        {
            try
            {
                var result = await client.ConnectAsync(GetMQttClientOptions());
            }
            catch (Exception exception)
            {
                //_logger.LogError(exception, "### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }
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

        public abstract void OnInitialize();

        private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            await OnDisconnected();
        }

        private async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            await OnConnected();
            throw new NotImplementedException();
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
