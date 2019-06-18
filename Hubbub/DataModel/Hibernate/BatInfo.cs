using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class BatInfo : DataObject
    {
        public virtual Deviceinfo Deviceinfo { get; set; }
        public virtual int? Heartbeat { get; set; }
        public virtual float? Chgpwrlmt { get; set; }
        public virtual float? Dhgpwrlmt { get; set; }
        public virtual float? Pvpwrhigh { get; set; }
        public virtual float? Pvpwrlow { get; set; }
        public virtual int? Status { get; set; }
        public virtual int? Fault1 { get; set; }
        public virtual int? Fault2 { get; set; }
        public virtual int? Fault3 { get; set; }
        public virtual string BatInfocol { get; set; }
    }
}
