using System;
using System.Text;
using System.Collections.Generic;


namespace DataModel {
    
    public class Deviceinfo {
        public Deviceinfo() { }
        public virtual int Uniqueid { get; set; }
       

        public virtual ISet<BatInfo> BatInfos { get; set; } = new HashSet<BatInfo>();
    }
}
