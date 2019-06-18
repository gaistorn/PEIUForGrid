using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DataModel
{
    public class PacketReader
    {
        byte[] data = null;
        int idx = 0;
        public PacketReader(byte[] array)
        {
            data = array;
        }

        public long ReadLong()
        {
            byte[] buffer = ReadPacket<long>();
            long value = BitConverter.ToInt64(buffer, 0);
            return value;
        }

        public string ReadString(int Length)
        {
            byte[] buffer = ReadPacket(Length);
            string value = Encoding.ASCII.GetString(buffer);
            return value;
        }

        public int ReadInt32()
        {
            byte[] buffer = ReadPacket<int>();
            int value = BitConverter.ToInt32(buffer);
            return value;
        }

        public ushort ReadUShort()
        {
            byte[] buffer = ReadPacket<ushort>();
            ushort value = BitConverter.ToUInt16(buffer);
            return value;
        }

        public IEnumerable<T> ByteToStructArray<T>(int count) where T : struct
        {
            int iSize = Marshal.SizeOf(typeof(T));
            for(int i=0;i< count; i ++)
            {
                byte[] buf = ReadPacket(iSize);
                T obj = ByteToStruct<T>(buf);
                yield return obj;
            }
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

        private byte[] ReadPacket(int size)
        {
            byte[] buffer = new byte[size];
            Array.Copy(data, idx, buffer, 0, size);
            idx = idx + size;
            return buffer;
        }

        private byte[] ReadPacket<T>() where T : IComparable
        {
            int longSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            byte[] buffer = new byte[longSize];
            Array.Copy(data, idx, buffer, 0, longSize);
            idx = idx + longSize;
            return buffer;
        }
    }
}
