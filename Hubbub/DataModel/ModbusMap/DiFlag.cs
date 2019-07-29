using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models {
    
    public class DiFlag {
        public virtual int No { get; set; }
        public virtual ushort BitValue { get; set; }
        public virtual string BitName { get; set; }
        public virtual int? RefDiMap { get; set; }
        public virtual ushort? EventCode { get; set; }

        public virtual bool IsActivate(ushort value)
        {
            return (value & BitValue) == BitValue;
        }
    }

}
