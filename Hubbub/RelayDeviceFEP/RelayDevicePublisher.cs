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
using StackExchange.Redis;
using EasyModbus;

namespace RelayDeviceFEP
{
    public class RelayDevicePublisher : AbsMqttPublisher, IDisposable
    {
        readonly RelayDataInformation relayDataInformation;
        readonly MysqlDataAccess mysqlDataAccess;
         EasyModbus.ModbusClient pvClient;
         EasyModbus.ModbusClient essClient;
        readonly ModbusConfig pvmodbusConfig;
        readonly ModbusConfig essmodbusConfig;
        readonly IDatabaseAsync redisDb;
        readonly ILogger logger;
        public RelayDevicePublisher(RelayDataInformation dataInformation, ModbusConfig pvmodbusConfig, ModbusConfig essmodbusConfig, MysqlDataAccess dataAccess, ConnectionMultiplexer connectionMultiplexer, ILogger logger)
        {
            this.relayDataInformation = dataInformation;
            this.pvmodbusConfig = pvmodbusConfig;
            this.essmodbusConfig = essmodbusConfig;
            this.logger = logger;
            this.mysqlDataAccess = dataAccess;
            redisDb = connectionMultiplexer.GetDatabase(1);
        }

        //protected override void OnInitialize()
        //{
        //    base.OnInitialize();
        //    InitializeModbus();
        //}

        private void InitializeModbus(ref EasyModbus.ModbusClient Client, ModbusConfig modbusConfig)
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

        protected override string GetMqttPublishTopicName()
        {
            //if (string.IsNullOrEmpty(DeviceId) || SiteId == -1)
            //    throw new Exception("DeviceId 또는 SiteId가 설정되지 않았습니다");
            return $"hubbub/{relayDataInformation.SiteId}/{relayDataInformation.DeviceName}/AI";
        }

        public async Task RunningAsync(CancellationToken cancellationToken)
        {
            DateTime triggerTime = DateTime.MinValue;
            InitializeModbus(ref pvClient, pvmodbusConfig);
            InitializeModbus(ref essClient, essmodbusConfig);
            while (true)
            {
                //if (triggerTime > DateTime.Now)
                //    continue;
                //triggerTime = DateTime.Now.Add(modbusConfig.Interval);

                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    Task[] client_access_task = new Task[]
                    {
                        TryConnect(pvClient, cancellationToken),
                        TryConnect(essClient, cancellationToken)
                    };
                    Task.WaitAll(client_access_task);


                    int[] ess_read_values = essClient.ReadInputRegisters(28, 30);
                    int[] pv_read_values = pvClient.ReadInputRegisters(28, 16);

                    //int[] trms = pvClient.ReadInputRegisters(538, 2);

                    //float trms_value = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { trms[0], trms[1] });
                    //logger.Info($"trms_value = {trms_value}");

                    float pcs_freq;
                    float pcs_actPwr;
                    float pcs_rctPwr;
                    float pcs_appPwr;
                    float pcs_act_forward_high;
                    float pcs_act_forward_low;

                    float pcs_act_reverse_high;
                    float pcs_act_reverse_low;

                    float pv_freq;
                    float pv_actPwr;
                    float pv_rctPwr;
                    float pv_appPwr;
                    float pv_act_forward_high;
                    float pv_act_forward_low;
                    
                    ReadValues(ess_read_values, out pcs_freq, out pcs_actPwr, out pcs_rctPwr, out pcs_appPwr, out pcs_act_forward_high, out pcs_act_forward_low, out pcs_act_reverse_high, out pcs_act_reverse_low);
                    ReadValues(pv_read_values, out pv_freq, out pv_actPwr, out pv_rctPwr, out pv_appPwr, out pv_act_forward_high, out pv_act_forward_low);

                    double pv_accum = pv_act_forward_high + (pv_act_forward_low / 10000);

                    double ess_total_chg = pcs_act_forward_high + (pcs_act_forward_low / 10000);
                    double ess_total_dhg = pcs_act_reverse_high + (pcs_act_reverse_low / 10000);

                    Console.WriteLine("High: " + pv_act_forward_high + " log: " + pv_act_forward_low + " accum: " + pv_accum);

                    await InsertDbAsync(pcs_freq, pv_actPwr, pv_accum, pcs_actPwr, ess_total_chg, ess_total_dhg, DateTime.Now, cancellationToken);
                    //await PublishEvent(freq, actPwr, rctPwr, appPwr, act_forward_high, act_forward_low, DateTime.Now, cancellationToken);
                    await Task.Delay(essmodbusConfig.Interval, cancellationToken);
                    
                }
                catch(Exception ex)
                {
                    logger.Error(ex, ex.Message);
                    essClient.Disconnect();
                    pvClient.Disconnect();
                }

            }
        }

        private void ReadValues(int[] read_values, out float Frequency, out float actPower, out float rctPower, out float appPower, out float act_forward_high, out float act_forward_low)
        {
            Frequency = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[0], read_values[1] });
            //actPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[2], read_values[3] });
            actPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[4], read_values[5] });
            rctPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[6], read_values[7] });
            appPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[8], read_values[9] });
            act_forward_high = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[10], read_values[11] });
            act_forward_low = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[12], read_values[13] });
        }

        private void ReadValues(int[] read_values, out float Frequency, out float actPower, out float rctPower, out float appPower, out float act_forward_high, out float act_forward_low, out float act_reverse_high, out float act_reverse_low)
        {
            Frequency = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[0], read_values[1] });
            //actPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[2], read_values[3] });
            actPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[4], read_values[5] });
            rctPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[6], read_values[7] });
            appPower = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[8], read_values[9] });
            act_forward_high = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[10], read_values[11] });
            act_forward_low = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[12], read_values[13] });
            // 14 15 = React Fwd High Lead
            // 16 17 = React Fwd Low Lead
            // 18 19 = React Fwd High Lag
            // 20 21 = React Fwd Low Lag
            // 22 23 = App Fwd High
            // 24 25 = App Fwd Log
            act_reverse_high = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[26], read_values[27] });
            act_reverse_low = EasyModbus.ModbusClient.ConvertRegistersToFloat(new int[] { read_values[28], read_values[29] });

        }

        private async Task InsertDbAsync(float Frequency, 
            float pv_act, double pv_accum, float pcs_act, double pcs_chg_Accum, double pcs_dhg_Accum,
            DateTime timestamp, CancellationToken token)
        {
            try
            {
                using(var session = mysqlDataAccess.SessionFactory.OpenStatelessSession())
                using(var trans = session.BeginTransaction())
                {
                    TbStatus status = new TbStatus();
                    status.Date = timestamp;
                    status.Pv = pv_act / 1000;
                    status.PvEng = pv_accum / 1000;
                    status.Ess = pcs_act / 1000;
                    status.EssCh = pcs_chg_Accum / 1000;
                    status.EssDch = pcs_dhg_Accum / 1000;
                    float[] socs = new float[4];
                    float[] acts = new float[4];
                    float[] pv_today_eng = new float[4];
                    int[] pcsstatus = new int[4];
                    for (int i = 1; i < 5; i++)
                    {
                        string redisKey = $"6.JeJuGridPcs{i}";
                        float soc1 = (float)await redisDb.HashGetAsync(redisKey, "bms_soc");
                        float act = (float)await redisDb.HashGetAsync(redisKey, "actPwrKw");
                        pv_today_eng[i - 1] = (float)await redisDb.HashGetAsync(redisKey, "TODAY_POWER");
                        pcsstatus[i-1] = (int)await redisDb.HashGetAsync(redisKey, "pcsstatus");

                        socs[i - 1] = soc1;
                        acts[i - 1] = act;

                    }
                    status.Soc1 = socs[0];
                    status.Soc2 = socs[1];
                    status.Soc3 = socs[2];
                    status.Soc4 = socs[3];
                    status.Stat1 = GetStat(pcsstatus[0]);
                    status.Stat2 = GetStat(pcsstatus[1]);
                    status.Stat3 = GetStat(pcsstatus[2]);
                    status.Stat4 = GetStat(pcsstatus[3]);
                    //status.Pv1_Today_Eng = pv_today_eng[0];
                    //status.Pv2_Today_Eng = pv_today_eng[1];
                    //status.Pv3_Today_Eng = pv_today_eng[2];
                    //status.Pv4_Today_Eng = pv_today_eng[3];
                    await session.InsertAsync(status);
                    await trans.CommitAsync();
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        private string GetStat(int stat)
        {
            if((stat & 2) == 2)
            {
                // Running
                if((stat & 32) == 32)
                {
                    return "충전중";
                }
                else if((stat & 64) == 64)
                {
                    return "방전중";
                }
                else
                {
                    return "대기";
                }
            }
            else
            {
                return "정지";
            }
        }

        public Task PublishEvent(float Frequency, float actPower, float rctPower, float appPower, float act_forward_high, float act_forward_low, DateTime timestamp, CancellationToken token)
        {
            JObject newObj = new JObject();
            newObj.Add("groupid", relayDataInformation.GroupId);
            newObj.Add("groupname", relayDataInformation.GroupName);
            newObj.Add("deviceId", relayDataInformation.DeviceName);
            newObj.Add("normalizedeviceid", relayDataInformation.DeviceName);
            newObj.Add("siteId", relayDataInformation.SiteId);
            newObj.Add("timestamp", timestamp.ToString("yyyyMMddHHmmss"));
            newObj.Add("freq", Frequency);
            newObj.Add("actPwrW", actPower);
            newObj.Add("rctPwrVar", rctPower);
            newObj.Add("appPwrVa", appPower);
            return base.PublishMessageAsync(newObj.ToString(), token);
        }

        public void Dispose()
        {
            if (essClient != null)
                essClient.Disconnect();

            if (pvClient != null)
                pvClient.Disconnect();


        }
    }
}
