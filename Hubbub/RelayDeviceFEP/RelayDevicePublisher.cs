using FireworksFramework.Mqtt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelayDeviceFEP
{
    public class RelayDevicePublisher : AbsMqttPublisher, IDisposable
    {
        readonly RelayDataInformation relayDataInformation;
        static EasyModbus.ModbusClient Client;
        readonly ModbusConfig modbusConfig;
        readonly ILogger logger;
        public RelayDevicePublisher(RelayDataInformation dataInformation, ModbusConfig modbusConfig, ILogger logger)
        {
            this.relayDataInformation = dataInformation;
            this.modbusConfig = modbusConfig;
            this.logger = logger;
        }

        //protected override void OnInitialize()
        //{
        //    base.OnInitialize();
        //    InitializeModbus();
        //}

        private void InitializeModbus()
        {
            if (string.IsNullOrEmpty(modbusConfig.SerialPort))
                Client = new EasyModbus.ModbusClient(modbusConfig.IpAddress, modbusConfig.Port);
            else
            {
                Client = new EasyModbus.ModbusClient(modbusConfig.SerialPort);
                Client.Baudrate = modbusConfig.BaudRate;
                Client.StopBits = modbusConfig.StopBits;
                Client.Parity = modbusConfig.Parity;
            }
            Client.ConnectedChanged += Client_ConnectedChanged;
           
        }


        private async Task TryConnect(EasyModbus.ModbusClient client, CancellationToken token)
        {
            int cnt = 1;
            
            while (client.Connected == false)
            {
                try
                {
                    client.Connect();
                    //client.Disconnect();
                    if (client.Connected)
                    {
                        string connectingInfo = string.IsNullOrEmpty(client.IPAddress) ? $"RTU ({client.SerialPort})" : $"TCP ({client.IPAddress}:{client.Port})";
                        this.logger.Info($"Connecting to Modbus slave at {connectingInfo}");
                    }
                    else
                        throw new TimeoutException("Connect error");
                    //await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, ex.Message);
                    this.logger.Info($"Attempting connection to Modbus Slave...Attempt #{cnt++}");
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
        }

        private void Client_ConnectedChanged(object sender)
        {
            EasyModbus.ModbusClient cl = (EasyModbus.ModbusClient)sender;
            if (cl.Connected == false)
            {
                //Task t = TryConnect()
            }
        }

        protected override string GetMqttPublishTopicName()
        {
            //if (string.IsNullOrEmpty(DeviceId) || SiteId == -1)
            //    throw new Exception("DeviceId 또는 SiteId가 설정되지 않았습니다");
            return $"hubbub/{relayDataInformation.SiteId}/{relayDataInformation.DeviceName}/AI";
        }

        public async Task RunningAsync(CancellationToken cancellationToken)
        {
            DateTime triggerTime = DateTime.MinValue;
            InitializeModbus();
            while (true)
            {
                //if (triggerTime > DateTime.Now)
                //    continue;
                //triggerTime = DateTime.Now.Add(modbusConfig.Interval);

                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await TryConnect(Client, cancellationToken);
                    int[] read_values = Client.ReadInputRegisters(28, 10);
                    logger.Info(string.Join(" ", read_values.Select(x => x.ToString())));
                    float freq = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[0], read_values[1] });
                    float pf = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[2], read_values[3] });
                    float actPwr = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[4], read_values[5] });
                    float rctPwr = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[6], read_values[7] });
                    float appPwr = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[8], read_values[9] });

                    await PublishEvent((Frequency: freq, actPower: actPwr, rctPower: rctPwr, appPower: appPwr), DateTime.Now, cancellationToken);
                    await Task.Delay(modbusConfig.Interval, cancellationToken);
                    
                }
                catch(Exception ex)
                {
                    Client.Disconnect();
                }

            }
            
        }

        public Task PublishEvent((float Frequency, float actPower, float rctPower, float appPower) RelayData, DateTime timestamp, CancellationToken token)
        {
            JObject newObj = new JObject();
            newObj.Add("groupid", relayDataInformation.GroupId);
            newObj.Add("groupname", relayDataInformation.GroupName);
            newObj.Add("deviceId", relayDataInformation.DeviceName);
            newObj.Add("normalizedeviceid", relayDataInformation.DeviceName);
            newObj.Add("siteId", relayDataInformation.SiteId);
            newObj.Add("timestamp", timestamp.ToString("yyyyMMddHHmmss"));
            newObj.Add("freq", RelayData.Frequency);
            newObj.Add("actPwrW", RelayData.actPower);
            newObj.Add("rctPwrVar", RelayData.rctPower);
            newObj.Add("appPwrVa", RelayData.appPower);
            return base.PublishMessageAsync(newObj.ToString(), token);
        }

        public void Dispose()
        {
            if (Client != null)
                Client.Disconnect();
            
            
        }
    }
}
