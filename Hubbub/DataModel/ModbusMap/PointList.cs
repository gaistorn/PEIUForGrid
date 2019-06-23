using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataModel {

    public class PointList : IModbusValue
    {
        public virtual int ID { get; set; }
        public virtual string Unit { get; set; }
        public virtual float? Scale { get; set; }
        public virtual string Description { get; set; }
        public virtual ushort Address { get; set; }
        public virtual int Level { get; set; }
        public virtual int TypeCode
        {
            get
            {
                return DataType.TypeCode;
            }
        }
        //public virtual int Type { get; set; }
        public virtual short Disable { get; set; }
        public virtual int GroupId { get; set; }
        public virtual string GroupName { get; set; }
        public virtual string Name { get; set; }
        //public virtual int ModbusFamilyId { get; set; }
        public virtual int IOType { get; set; }

        public virtual IList<DiFlag> Flags { get; set; }

        [JsonIgnore]
        public virtual DataType DataType {get;set;}

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


        private  void SetValue(object value)
        {
            _value = ConvertValue(value);
        }

        private object _value;
        public virtual object Value
        {
            set
            {
                SetValue(value);
            }get
            {
                return _value;
            }
        }

        [JsonIgnore]
        public virtual ModbusFamily ModbusFamily { get; set; }

        public virtual object ConvertValue(dynamic x)
        {
            if (DataType.TypeCode == 1 || Scale == 0 || Scale == 1)
                return x;
            else
                return x * Scale;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
