using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PEIU.Models
{
    public class PacketWriter
    {
        ArrayList buffer = new ArrayList(); 

        public PacketWriter()
        {

        }

        public byte[] GetBuffer()
        {
            return (byte[])buffer.ToArray((typeof(byte)));
        }

        public int WriteString(string value)
        {
            byte[] buf = Encoding.ASCII.GetBytes(value);
            buffer.AddRange(buf);
            return buf.Length;

        }

        public void WriteLong(long value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            buffer.AddRange(buf);
        }

        public void WriteInt32(Int32 value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            buffer.AddRange(buf);
        }

        public void WriteUShort(ushort value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            buffer.AddRange(buf);
        }

        public void WriteStructures<T>(IEnumerable<T> values) where T : struct
        {
            foreach(T value in values)
            {
                byte[] buf = ToByteArray<T>(value);
                buffer.AddRange(buf);
            }
        }

        public byte[] ToByteArray<T>(T value)
        {
            byte[] arr = null;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                Int16 size = (Int16)Marshal.SizeOf(value);
                arr = new byte[size];
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);

            }
            catch (Exception e)
            {
                // 예외 발생
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }
    }
}
