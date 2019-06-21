using DataModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NModbus;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PEIU.Hubbub
{
    public interface IModbusFactory : IDisposable
    {
        TcpClient GetTcpClient();
        IModbusMaster GetModbusMaster();
        bool TryConnectModbus(ModbusSystem config);
        bool ReconnectWhenDisconnected();
        // Task<MqttRegister[]> ReadModbus(SlaveInfoYaml slaves);
        JObject ReadModbusToJson(GroupPoint slaves, out HashEntry[] hashEntry);
        Task<List<DiMap>> ReadModbusEvent(EventGroupPoint map);
        //Task<IEnumerable<MqttInlineRegisterJsonModel>> ReadModbusToArray(SlaveInfoYaml slaves);
    }

    public class ModbusConnectionFactory : IModbusFactory
    {
        ILogger logger;
        TcpClient tcpClient;
        IModbusMaster master;
        ModbusSystem _config;
        readonly object lockerObj = new object(); 
        public ModbusConnectionFactory(ILoggerFactory loggerFactory, ModbusSystem modbusConfig)
        {
            //config = configYaml;
            logger = loggerFactory.CreateLogger<ModbusConnectionFactory>();
            _config = modbusConfig;
        }

        public void Dispose()
        {
            if (GetTcpClient() != null)
                GetTcpClient().Dispose();
        }

        public IModbusMaster GetModbusMaster()
        {
            return master;
        }

        public TcpClient GetTcpClient()
        {
            return tcpClient;
        }

        //public async Task<IEnumerable<MqttInlineRegisterJsonModel>> ReadModbusToArray(SlaveInfoYaml slaves)
        //{
        //    ushort minAddr = slaves.registers.Min(x => x.address);
        //    ushort maxAddr = slaves.registers.Max(x => x.address);
        //    minAddr = (ushort)(minAddr - 1);
        //    ushort pointCnt = (ushort)((maxAddr - minAddr) + 1);
        //    ushort[] datas = null;
        //    lock (lockerObj)
        //    {
        //        switch (slaves.io_type)
        //        {
        //            case DataModel.modbus_io.ANALOG_INPUT:
        //                datas = master.ReadInputRegisters(slaves.deviceId, minAddr, pointCnt);
        //                break;
        //            case DataModel.modbus_io.HOLDING_REGISTER:
        //                datas = master.ReadHoldingRegisters(slaves.deviceId, minAddr, pointCnt);
        //                break;
        //        }
        //        Console.WriteLine("READ MODBUS_INPUT_REGISTER");
        //    }

        //    DateTime timeStamp = DateTime.Now;
        //    List<MqttRegisterJsonModel> registers = new List<MqttRegisterJsonModel>();
        //    foreach (SlaveInfoRegisterYaml register in slaves.registers)
        //    {
        //        Console.WriteLine("READ REGISTER: " + register.name);
        //        int addr = (register.address - 1) - minAddr;
        //        byte[] buffer = GetReadBytes(register, addr, datas);
        //        object value = 0f;
        //        switch (register.type)
        //        {
        //            case DataModel.modbus_type.DT_SIGNED:
        //                if (register.length > 1)
        //                    value = BitConverter.ToInt32(buffer);
        //                else
        //                    value = BitConverter.ToInt16(buffer);
        //                break;
        //            case modbus_type.DT_UNSIGNED:
        //                if (register.length > 1)
        //                    value = BitConverter.ToUInt32(buffer);
        //                else
        //                    value = BitConverter.ToUInt16(buffer);
        //                break;
        //            case modbus_type.DT_FLOAT:
        //                value = BitConverter.ToSingle(buffer);
        //                break;
        //        }
        //        registers.Add(register.CreateJsonModel(value));
        //        //registers.Add(register.ConvertRegister(Convert.ToSingle(value)));
        //    }
        //    parent.Registers = registers.ToArray();
        //    return parent;
        //}

        public async Task<List<DiMap>> ReadModbusEvent(EventGroupPoint map)
        {
            List<DiMap> result = new List<DiMap>();
            if (master == null) return result;
            ushort[] datas = await master.ReadHoldingRegistersAsync(map.SlaveId, map.StartAddress, (ushort)map.DigitalPoints.Count);
            int idx = 0;
            foreach (DiMap m in map.DigitalPoints)
            {
                m.Value = datas[idx++];
                result.Add(m);
            }
            return result;
        }

        public JObject ReadModbusToJson(GroupPoint slaves, out HashEntry[] hashEntries)
        {
            JObject datarow = new JObject();
            List<HashEntry> entries = new List<HashEntry>();
            hashEntries = entries.ToArray();
            try
            {
               
                if (master == null)
                    return datarow;
                ushort minAddr = slaves.AiMaps.Min(x => x.Address);
                ushort maxAddr = slaves.AiMaps.Max(x => x.Address);
                //minAddr = (ushort)(minAddr - 1);
                ushort pointCnt = (ushort)((maxAddr - minAddr) + 1);
                ushort[] datas = null;
                lock (lockerObj)
                {


                    switch ((DataModel.modbus_io)slaves.IoType)
                    {
                        case DataModel.modbus_io.ANALOG_INPUT:
                            datas = master.ReadInputRegisters(slaves.SlaveId, minAddr, pointCnt);
                            break;
                        case DataModel.modbus_io.HOLDING_REGISTER:
                            datas = master.ReadHoldingRegisters(slaves.SlaveId, minAddr, pointCnt);
                            break;
                    }
                }
                DateTime timeStamp = DateTime.Now;
                datarow.Add("groupid", slaves.GroupId);
                datarow.Add("groupname", slaves.GroupName);
                datarow.Add("deviceId", slaves.DeviceUniqueId);
                entries.Add(new HashEntry("timestamp", timeStamp.ToString()));
                datarow.Add("timestamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
                int bitIdx = 0;
                foreach (AiMap register in slaves.AiMaps)
                {
                    if (register.Disable == 1)
                        continue;
                    //Console.WriteLine("READ REGISTER: " + register.Name);
                    byte[] buffer = GetReadBytes(register.DataType, bitIdx, datas);
                    bitIdx += register.DataType.Size;
                    dynamic value = 0f;
                    switch ((DataModel.modbus_type)register.DataType.TypeCode)
                    {
                        case modbus_type.DT_BOOLEAN:
                            value = BitConverter.ToBoolean(buffer);
                            break;
                        case DataModel.modbus_type.DT_INT32:
                            value = BitConverter.ToInt32(buffer);
                            break;
                        case modbus_type.DT_INT16:
                            value = BitConverter.ToInt16(buffer);
                            break;
                        case modbus_type.DT_UINT32:
                            value = BitConverter.ToUInt32(buffer);
                            break;
                        case modbus_type.DT_UINT16:
                            value = BitConverter.ToUInt16(buffer);
                            break;
                        case modbus_type.DT_FLOAT:
                            value = BitConverter.ToSingle(buffer);
                            break;
                    }

                     value = register.ConvertValue(value);
                    entries.Add(new HashEntry(register.Name, value.ToString()));
                    datarow.Add(register.Name, value.ToString());
                    //registers.Add(register.ConvertRegister(Convert.ToSingle(value)));
                }
                hashEntries = entries.ToArray();
                return datarow;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return null;
            }
        
        }

        //public async Task<MqttRegister[]> ReadModbus(SlaveInfoYaml slaves)
        //{
        //    ushort minAddr = slaves.registers.Min(x => x.address);
        //    ushort maxAddr = slaves.registers.Max(x => x.address);
        //    minAddr = (ushort)(minAddr - 1);
        //    ushort pointCnt =(ushort) ((maxAddr - minAddr) + 1);
        //    ushort[] datas = null;
        //    lock (lockerObj)
        //    {
        //        switch (slaves.io_type)
        //        {
        //            case DataModel.modbus_io.ANALOG_INPUT:
        //                datas =  master.ReadInputRegisters(slaves.deviceId, minAddr, pointCnt);
        //                break;
        //            case DataModel.modbus_io.HOLDING_REGISTER:
        //                datas =  master.ReadHoldingRegisters(slaves.deviceId, minAddr, pointCnt);
        //                break;
        //        }
        //        Console.WriteLine("READ MODBUS_INPUT_REGISTER");
        //    }

        //    List<MqttRegister> registers = new List<MqttRegister>();
        //    foreach(SlaveInfoRegisterYaml register in slaves.registers)
        //    {
        //        Console.WriteLine("READ REGISTER: " + register.name);
        //        int addr = (register.address - 1) - minAddr;
        //        byte[] buffer = GetReadBytes(register, addr, datas);
        //        object value = 0f;
        //        switch (register.type)
        //        {
        //            case DataModel.modbus_type.DT_SIGNED:
        //                if (register.length > 1)
        //                    value = BitConverter.ToInt32(buffer);
        //                else
        //                    value = BitConverter.ToInt16(buffer);
        //                break;
        //            case modbus_type.DT_UNSIGNED:
        //                if (register.length > 1)
        //                    value = BitConverter.ToUInt32(buffer);
        //                else
        //                    value = BitConverter.ToUInt16(buffer);
        //                break;
        //            case modbus_type.DT_FLOAT:
        //                value = BitConverter.ToSingle(buffer);
        //                break;
        //        }
        //        registers.Add(register.ConvertRegister(Convert.ToSingle(value)));
        //    }
        //    return registers.ToArray();
        //}

        private byte[] GetReadBytes(DataType register, int src, ushort[] datas)
        {
            byte[] buffer = new byte[register.Size * 2];
            int idx = register.Size > 1 ? 2 : 0;

            byte[] test = BitConverter.GetBytes(56.119999f);
            ushort[] svalue = new ushort[register.Size];
            Array.Copy(datas, src, svalue, 0, register.Size);
            foreach (ushort usvalue in svalue)
            {
                byte[] bits = BitConverter.GetBytes(usvalue);
                Array.Copy(bits, 0, buffer, idx, 2);
                idx -= 2;
            }
            return buffer;

        }

        UInt32 ConvertDecimalToUint32(string[] svalue)
        {
            byte[] fbits = new byte[4];
            uint idx = 2;
            foreach (string ushort_str in svalue)
            {
                short usvalue = short.Parse(ushort_str);
                byte[] bits = BitConverter.GetBytes(usvalue);
                Array.Copy(bits, 0, fbits, idx, 2);
                idx -= 2;
            }
            UInt32 value = BitConverter.ToUInt32(fbits, 0);
            return value;
        }


        float ConvertDecimalToIEEE754float(string[] svalue)
        {
            byte[] fbits = new byte[4];
            uint idx = 2;
            foreach (string ushort_str in svalue)
            {
                short usvalue = short.Parse(ushort_str);
                byte[] bits = BitConverter.GetBytes(usvalue);
                Array.Copy(bits, 0, fbits, idx, 2);
                idx -= 2;
            }

            //ushort v1 = ushort.Parse(svalue[0]);
            //ushort v2 = ushort.Parse(svalue[1]);
            //byte[] bits1 = BitConverter.GetBytes(v1);
            //byte[] bits2 = BitConverter.GetBytes(v2);



            //Array.Copy(bits2, 0, fbits, 0, 2);
            //Array.Copy(bits1, 0, fbits, 2, 2);
            float fvalue = BitConverter.ToSingle(fbits, 0);
            //byte[] realbits = BitConverter.GetBytes(12.25f);
            return fvalue;

        }

        public bool ReconnectWhenDisconnected()
        {
            try
            {
                if (_config == null)
                    return false;
                if (tcpClient == null || tcpClient.Connected == false)
                    TryConnectModbus(_config);
                return tcpClient != null && tcpClient.Connected;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public bool TryConnectModbus(ModbusSystem config)
        {
            try
            {
                if(tcpClient != null)
                {
                    tcpClient.Dispose();
                    tcpClient = null;
                    //master.Dispose();
                    //master = null;
                }
                _config = config;
                tcpClient = new TcpClient(config.IpAddress, config.PortNum);
                var factory = new ModbusFactory();
                
                master = factory.CreateMaster(tcpClient);
                
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "#### MODBUS CONNECTING FAILED ####");
                return false;
            }
        }
    }
}
