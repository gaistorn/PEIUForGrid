using System;
using System.Text;
using System.Collections.Generic;


namespace PEIU.Models {
    
    public class DataType {
        public virtual int TypeCode { get; set; }
        public virtual string TypeName { get; set; }
        public virtual int Size { get; set; }
        public virtual bool IsUnsigned { get; set; }
    }
}
