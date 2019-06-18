using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataModel
{


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MqttDataPacket
    {
        public int groupNameLength { get; internal set; }
        public string groupName { get; internal set; }
        public long unixtimeStamp;
        public int registerCnt;


        public MqttRegister[] registers;

        public void SetGroupName(string Name)
        {
            groupNameLength = Encoding.ASCII.GetByteCount(Name);
            groupName = Name;

        }

        public void SetDateTime(DateTimeOffset time)
        {
            unixtimeStamp = time.ToUnixTimeSeconds();
        }

        public byte[] ToByteArray()
        {

            PacketWriter writer = new PacketWriter();
            writer.WriteInt32(groupNameLength);
            writer.WriteString(groupName);
            writer.WriteLong(unixtimeStamp);
            writer.WriteInt32(registerCnt);
            writer.WriteStructures<MqttRegister>(registers);
            return writer.GetBuffer();
        }

        public static MqttDataPacket Parse(byte[] array)
        {
            PacketReader reader = new PacketReader(array);
            MqttDataPacket packet = new MqttDataPacket();
            packet.groupNameLength = reader.ReadInt32();
            packet.groupName = reader.ReadString(packet.groupNameLength);
            packet.unixtimeStamp = reader.ReadLong();
            packet.registerCnt = reader.ReadInt32();
            var registers = reader.ByteToStructArray<MqttRegister>(packet.registerCnt);
            packet.registers = registers.ToArray();
            return packet;

        }

        public static T ByteToStruct<T>(byte[] buffer) where T : struct
        {
            int iSize = Marshal.SizeOf(typeof(T));

            if (iSize > buffer.Length)
            {
                throw new Exception(string.Format("SIZE ERR(len:{0} sz:{1})", buffer.Length, iSize));
            }

            IntPtr ptr = Marshal.AllocHGlobal(iSize);
            Marshal.Copy(buffer, 0, ptr, iSize);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return obj;
        }
        //public DaegunPcsPacket Pcs;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MqttRegister
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string name;
        public ushort adddress;
        public byte length;
        public modbus_type type;
        public float scale;
        public float value;

        public MqttRegisterJsonModel CreateJsonModel(object value)
        {
            MqttRegisterJsonModel model = new MqttRegisterJsonModel();
            model.Name = name;
            model.Adddress = adddress;
            model.Length = length;
            model.Type = type;
            model.Scale = scale;
            model.Value = value;
            return model;
        }
    }

    public enum modbus_io : ushort
    {
        COIL_REGISTER = 0,
        HOLDING_REGISTER = 40000,
        ANALOG_INPUT = 30000,
        DIGITAL_INPUT = 10000
    }

    public enum modbus_type : byte
    {
        DT_BOOLEAN = 1,
        DT_UINT16 = 2,
        DT_INT16 = 3,
        DT_UINT32 = 4,
        DT_INT32 = 5,
        DT_FLOAT = 6,
        DT_UINT64 = 7,
        DT_INT64 = 8
    }
}
