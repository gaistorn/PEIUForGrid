using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class ModbusFamily
    {
        public virtual int ID { get; set; }
        public virtual string FamilyName { get; set; }
        public virtual bool Disable { get; set; }
        public virtual int SiteNo { get; set; }

        public virtual IList<PointList> PointList { get; set; }

        public virtual IList<ModbusSystem> ModbusSystems { get; set; }
    }
}
