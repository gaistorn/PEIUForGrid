using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class ModbusJsonGroupObject
    {
        public string GroupName { get; set; }
        public DateTime TimeStamp { get; set; }


    }

    public enum ModbusGroupType
    {
        ANALOGUE,
        STATUS,
        EVENT

    }

    public abstract class ModbusJsonObjectParent
    {
        public int DocumentAddress { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public ushort Address { get; set; }
        public int DataTypeCode { get; set; }
        public string DataType { get; set; }
    }

    public class ModbusJsonDigital : ModbusJsonObjectParent
    {
        public int Value { get; set; }
    }

    public class ModbusJsonAnalogue : ModbusJsonObjectParent
    {
        public string Unit { get; set; }
        public float Scale { get; set; }
        public int Type { get; set; }
        public object Value { get; private set; }
        public void SetValue(object x)
        {
            if (Type == 1 || Scale == 0 || Scale == 1)
                this.Value =  x;
            if (x is float)
                this.Value = (float)x * Scale;
            else
            {
                Int64 x64 = (Int64)x;
                this.Value = x64 * Scale;
            }
        }
    }

    public class ModbusJsonObject
    {
        public int DocumentAddress { get; set; }
        public string Unit { get; set; }
        public float Scale { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public ushort Address { get; set; }
        public int Type { get; set; }

        public string DataType { get; set; }
        public short Disable { get; set; }

        public virtual object ConvertValue(object x)
        {
            if (Type == 1 || Scale == 0 || Scale == 1)
                return x;
            if (x is float)
                return (float)x * Scale;
            else
            {
                Int64 x64 = (Int64)x;
                return x64 * Scale;
            }
        }
    }
}
