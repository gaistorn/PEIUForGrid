using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DataModel {

    //public class DiMap : IModbusValue
    //{
    //    public virtual int ID { get; set; }
    //    public virtual string Description { get; set; }
    //    public virtual short Address { get; set; }
    //    public virtual short Disable { get; set; }
    //    public virtual string Name { get; set; }
    //    public virtual int Modbus_Family_Id { get; set; }
    //    public virtual bool Event { get; set; }
    //    public virtual int Level { get; set; }
    //    public virtual int Group_Id { get; set; }
    //    public virtual string Group_Name { get; set; }
    //    public virtual int IO_Type { get; set; }
    //    public virtual int Type { get; set; }

    //    public virtual object Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //    public virtual int GroupId => throw new NotImplementedException();

    //    public virtual string GroupName => throw new NotImplementedException();

    //    public virtual int IOType => throw new NotImplementedException();

    //    public virtual int TypeSize => throw new NotImplementedException();

    //    public virtual int TypeCode => throw new NotImplementedException();

    //    ushort IModbusValue.Address => throw new NotImplementedException();
    //}

    public class DiMap : IModbusValue
    {
        public virtual int ID { get; set; }
        public virtual string Description { get; set; }
        public virtual ushort Address { get; set; }
        public virtual short Disable { get; set; }
        public virtual string Name { get; set; }
        //public virtual int ModbusFamilyId { get; set; }
        public virtual bool Event { get; set; }
        public virtual int Level { get; set; }
        public virtual int IOType { get; set; }
        public virtual int Type { get; set; }
        public virtual ushort Value { get; set; } = 0;
        //public virtual int EventGroupId { get; set; }
        public virtual IList<DiFlag> Flags { get; set; }
        public virtual int GroupId { get; set; }
        public virtual string GroupName { get; set; }

        [JsonIgnore]
        public virtual DataType DataType { get; set; }
        public virtual int TypeSize
        {
            get
            {
                if (DataType == null)
                    return 1;
                else
                    return DataType.Size;
            }
        }

        public virtual int TypeCode { get; set; }

        public virtual ModbusFamily ModbusFamily { get; set; }
        object IModbusValue.Value { get => Value; set => Value = (ushort)value; }
    }

    public class DiMapValue
    {
        public DiMap MapInfo { get; private set; }
        public ushort usValue { get; private set; }
       
    }
}
