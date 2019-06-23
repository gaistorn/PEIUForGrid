using DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NModbus;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Hubbub
{
    public interface IModbusFactory : IDisposable
    {
        //TcpClient GetTcpClient();
        //IModbusMaster GetModbusMaster();
        //bool TryConnectModbus(ModbusSystem config);
        //bool ReconnectWhenDisconnected();
        //// Task<MqttRegister[]> ReadModbus(SlaveInfoYaml slaves);
        //JObject ReadModbusToJson(ModbusFamily family, out HashEntry[] hashEntry);
        //void ReadModbusEvent(Modbus map)
        //Task<IEnumerable<MqttInlineRegisterJsonModel>> ReadModbusToArray(SlaveInfoYaml slaves);
        Task<bool> WriteMultipleRegisters(string DeviceName, ushort startAddress, params ushort[] values);
        Task<ushort[]> ReadHoldingRegisters(string DeviceName, ushort startAddress, ushort sizeOfLength);

        Task<bool> WriteMultipleCoilsAsync(string DeviceName, ushort startAddress, params bool[] values);
        int SiteNo { get; }
        Task ReadAnalogData(Func<IEnumerable<PointList>, ModbusSystem, CancellationToken, Task> readAi, CancellationToken token);
        Task ReadDigitalData(Func<IEnumerable<PointList>, ModbusSystem, CancellationToken, Task> readDi, CancellationToken token);
    }

    public class ModbusConnectionFactory : IModbusFactory
    {
        ILogger logger;
        Modbus[] modbuses;
         ModbusFamily modbusFamily;
        readonly object lockerObj = new object();
        readonly TimeSpan AI_Interval;
        readonly TimeSpan DI_Interval;

        public int SiteNo
        {
            get
            {
                return modbusFamily.SiteNo;
            }
        }

        public ModbusConnectionFactory(ILoggerFactory loggerFactory, ModbusFamily modbusConfig, IConfiguration Configuration)
        { 
            //config = configYaml;
            logger = loggerFactory.CreateLogger<ModbusConnectionFactory>();
            modbusFamily = modbusConfig;
            
            modbuses = ConvertModbus(modbusConfig);
            AI_Interval = Configuration.GetSection("ModbusConfig:IntervalAI").Get<TimeSpan>();
            DI_Interval = Configuration.GetSection("ModbusConfig:IntervalDI").Get<TimeSpan>();
        }

        public async Task<ushort[]> ReadHoldingRegisters(string DeviceName, ushort startAddress, ushort sizeOfLength)
        {
            var mod = modbuses.FirstOrDefault(x => x.DeviceName == DeviceName);
            if (mod == null)
                return null;
            ushort[] values = await mod.ReadHoldingRegistersAsync(startAddress, sizeOfLength);
            return values;
        }

        public async Task<bool> WriteMultipleRegisters(string DeviceName, ushort startAddress, params ushort[] values)
        {
            var mod = modbuses.FirstOrDefault(x => x.DeviceName == DeviceName);
            if (mod == null)
                return false;
            await mod.WriteMultipleRegistersAsync(startAddress, values);
            return true;
        }

        public async Task<bool> WriteMultipleCoilsAsync(string DeviceName, ushort startAddress, params bool[] values)
        {
            var mod = modbuses.FirstOrDefault(x => x.DeviceName == DeviceName);
            if (mod == null)
                return false;
            await mod.WriteMultipleCoilsAsync(startAddress, values);
            return true;
        }

        private Modbus[] ConvertModbus(ModbusFamily family)
        {
            List<Modbus> modbus = new List<Modbus>();
            foreach(ModbusSystem system in  family.ModbusSystems)
            {
                Modbus insertBus = CreateModbus(system, family.PointList.Where(x=>x.Level == 0), family.PointList.Where(x=>x.Level != 0));
                modbus.Add(insertBus);
            }
            return modbus.ToArray();
        }

        public void Dispose()
        {
            if(modbuses != null)
            {
                foreach (Modbus bus in modbuses)
                    bus.Dispose();
            }
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

        public async Task ReadAnalogData(Func<IEnumerable<PointList>, ModbusSystem, CancellationToken, Task> readAi, CancellationToken token)
        {
            foreach (Modbus modbus in modbuses)
            {
                try
                {
                    await UpdateValues(modbus, modbus.AiMaps, AI_Interval, token);
                    await readAi(modbus.AiMaps, modbus.ModbusSystem, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task ReadDigitalData(Func<IEnumerable<PointList>, ModbusSystem, CancellationToken, Task> readDi, CancellationToken token)
        {
            foreach (Modbus modbus in modbuses)
            {
                try
                {
                    await UpdateValues(modbus, modbus.DiMaps, AI_Interval, token);
                    await readDi(modbus.DiMaps, modbus.ModbusSystem, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }

        private async Task UpdateValues<T>(Modbus modbus, IEnumerable<T> mapitem, TimeSpan Delay, CancellationToken token) where T : IModbusValue
        {
            var master = modbus.Master;
            var groups = mapitem.OrderBy(x => x.Address).GroupBy(x => x.GroupId, y => y);
            foreach(var group_row in groups)
            {
                if (token.IsCancellationRequested)
                    return;
                
                int start_address = group_row.Min(x => x.Address);
                int count = group_row.Sum(x => x.TypeSize);
                DataModel.modbus_io ioType = (DataModel.modbus_io)group_row.FirstOrDefault().IOType;
                ushort[] di_values = null;
                switch (ioType)
                {
                    case modbus_io.HOLDING_REGISTER:
                        di_values = await master.ReadHoldingRegistersAsync(modbus.SlaveId, (ushort)start_address, (ushort)count);
                        break;
                    case modbus_io.ANALOG_INPUT:
                        di_values = await master.ReadHoldingRegistersAsync(modbus.SlaveId, (ushort)start_address, (ushort)count);
                        break;
                }
                
                int idx_value = 0;
                foreach (T group_in_map in group_row)
                {
                    if (group_in_map.Disable != 0)
                        continue;
                    ushort[] value_buffer = new ushort[group_in_map.TypeSize];
                    Array.Copy(di_values, idx_value, value_buffer, 0, value_buffer.Length);
                    byte[] buffer = ConvertToArray(value_buffer);
                    dynamic value = 0;
                    DataModel.modbus_type pt = (DataModel.modbus_type)group_in_map.TypeCode;
                    switch (pt)
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
                    group_in_map.Value = value;
                    idx_value += group_in_map.TypeSize;
                }
                await Task.Delay(Delay);
            }
        }

        //public JObject ReadModbusToJson(Modbus map, out HashEntry[] hashEntries)
        //{

        //    JObject datarow = new JObject();
        //    List<HashEntry> entries = new List<HashEntry>();
        //    hashEntries = entries.ToArray();
        //    try
        //    {
               
        //        if (map.Master == null)
        //            return datarow;


        //        DateTime timeStamp = DateTime.Now;
        //        datarow.Add("groupid", slaves.GroupId);
        //        datarow.Add("groupname", slaves.GroupName);
        //        datarow.Add("deviceId", slaves.DeviceUniqueId);
        //        entries.Add(new HashEntry("timestamp", timeStamp.ToString()));
        //        datarow.Add("timestamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //        int bitIdx = 0;
        //        foreach (AiMap register in slaves.AiMaps)
        //        {
        //            if (register.Disable == 1)
        //                continue;
        //            //Console.WriteLine("READ REGISTER: " + register.Name);
        //            byte[] buffer = GetReadBytes(register.DataType, bitIdx, datas);
        //            bitIdx += register.DataType.Size;
        //            dynamic value = 0f;
        //            switch ((DataModel.modbus_type)register.DataType.TypeCode)
        //            {
        //                case modbus_type.DT_BOOLEAN:
        //                    value = BitConverter.ToBoolean(buffer);
        //                    break;
        //                case DataModel.modbus_type.DT_INT32:
        //                    value = BitConverter.ToInt32(buffer);
        //                    break;
        //                case modbus_type.DT_INT16:
        //                    value = BitConverter.ToInt16(buffer);
        //                    break;
        //                case modbus_type.DT_UINT32:
        //                    value = BitConverter.ToUInt32(buffer);
        //                    break;
        //                case modbus_type.DT_UINT16:
        //                    value = BitConverter.ToUInt16(buffer);
        //                    break;
        //                case modbus_type.DT_FLOAT:
        //                    value = BitConverter.ToSingle(buffer);
        //                    break;
        //            }

        //             value = register.ConvertValue(value);
        //            entries.Add(new HashEntry(register.Name, value.ToString()));
        //            datarow.Add(register.Name, value.ToString());
        //            //registers.Add(register.ConvertRegister(Convert.ToSingle(value)));
        //        }
        //        hashEntries = entries.ToArray();
        //        return datarow;
        //    }
        //    catch(Exception ex)
        //    {
        //        logger.LogError(ex, ex.Message);
        //        return null;
        //    }
        
        //}

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

        byte[] ConvertToArray(ushort[] svalues)
        {
            byte[] buffer = new byte[svalues.Length * 2];
            uint idx = (uint)buffer.Length - 2;
            foreach (ushort ushort_str in svalues)
            {
                short usvalue = Convert.ToInt16(ushort_str);
                byte[] bits = BitConverter.GetBytes(usvalue);
                Array.Copy(bits, 0, buffer, idx, 2);
                idx -= 2;
            }
            return buffer;
        }

        UInt32 ConvertDecimalToUint32(ushort[] svalue)
        {
            byte[] fbits = ConvertToArray(svalue);
            UInt32 value = BitConverter.ToUInt32(fbits, 0);
            return value;
        }


        float ConvertDecimalToIEEE754float(ushort[] svalue)
        {
            byte[] fbits = ConvertToArray(svalue);

            float fvalue = BitConverter.ToSingle(fbits, 0);
            //byte[] realbits = BitConverter.GetBytes(12.25f);
            return fvalue;

        }

        //UInt32 ConvertDecimalToUint32(ushort[] svalue)
        //{
        //    byte[] fbits = new byte[4];
        //    uint idx = 2;
        //    foreach (ushort ushort_str in svalue)
        //    {
        //        short usvalue = Convert.ToInt16(ushort_str);
        //        byte[] bits = BitConverter.GetBytes(usvalue);
        //        Array.Copy(bits, 0, fbits, idx, 2);
        //        idx -= 2;
        //    }
        //    UInt32 value = BitConverter.ToUInt32(fbits, 0);
        //    return value;
        //}


        //float ConvertDecimalToIEEE754float(ushort[] svalue)
        //{
        //    byte[] fbits = new byte[4];
        //    uint idx = 2;
        //    foreach (ushort ushort_str in svalue)
        //    {
        //        short usvalue = Convert.ToInt16(ushort_str);
        //        byte[] bits = BitConverter.GetBytes(usvalue);
        //        Array.Copy(bits, 0, fbits, idx, 2);
        //        idx -= 2;
        //    }

        //    float fvalue = BitConverter.ToSingle(fbits, 0);
        //    //byte[] realbits = BitConverter.GetBytes(12.25f);
        //    return fvalue;

        //}

        private Modbus CreateModbus(ModbusSystem config, IEnumerable<PointList> aiMaps, IEnumerable<PointList> diMaps)
        {
            TcpClient client = new TcpClient(config.IpAddress, config.PortNum);
            var factory = new ModbusFactory();
            IModbusMaster master = factory.CreateMaster(client);
            Modbus newBus = new Modbus(config, master, client, config.SlaveId, aiMaps, diMaps);
            return newBus;
        }

    }

    public class Modbus : IDisposable
    {
        public string DeviceName
        {
            get
            {
                return ModbusSystem.DeviceName;
            }
        }

        public int DeviceId
        {
            get
            {
                return ModbusSystem.DeviceId;
            }
        }

        public IModbusMaster Master { get; private set; }
        public TcpClient Client { get; private set; }
        public IEnumerable<PointList> AiMaps { get; private set; }
        public IEnumerable<PointList> DiMaps { get; private set; }
        public ModbusSystem ModbusSystem { get; private set; }
        public byte SlaveId { get; private set; }
        public Modbus(ModbusSystem modbusSystem, IModbusMaster master, TcpClient client, byte slaveId, IEnumerable<PointList> aiMaps, IEnumerable<PointList> diMaps)
        {
            this.Master = master;
            this.Client = client;
            AiMaps = aiMaps;
            DiMaps = diMaps;
            SlaveId = slaveId;
            ModbusSystem = modbusSystem;
        }

        public async Task<bool> WriteMultipleRegistersAsync(ushort startAddress, params ushort[] values)
        {
            if (TryConnectionModbus() == false)
                return false;
            await Master.WriteMultipleRegistersAsync(SlaveId, startAddress, values);
            return true;
            
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort count)
        {
            if (TryConnectionModbus() == false)
                return null;
            ushort[] values = await Master.ReadHoldingRegistersAsync(SlaveId, startAddress, count);
            return values;
        }

        public async Task<bool> WriteMultipleCoilsAsync(ushort startAddress, params bool[] values)
        {
            if (TryConnectionModbus() == false)
                return false;
            await Master.WriteMultipleCoilsAsync(SlaveId, startAddress, values);
            return true;
        }

        public bool TryConnectionModbus()
        {
            if (Client == null)
            {
                Client = new TcpClient(ModbusSystem.IpAddress, ModbusSystem.PortNum);
                var factory = new ModbusFactory();
                Master = factory.CreateMaster(Client);
            }
            else if (Client.Connected == false)
            {
                Client.Connect(ModbusSystem.IpAddress, ModbusSystem.PortNum);
            }
            return Client.Connected;
        }

        public void Dispose()
        {
            if (Client != null)
                Client.Dispose();
            if (Master != null)
                Master.Dispose();
        }
    }
}
