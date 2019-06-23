using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public interface IModbusValue
    {
        ushort Address { get; }
        object Value { get; set; }
        int GroupId { get;  }
        string GroupName { get; }
         int IOType { get;  }
        int TypeSize { get; }
        int TypeCode { get; }
        short Disable { get; }
    }
}
