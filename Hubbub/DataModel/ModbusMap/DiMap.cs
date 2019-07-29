using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PEIU.Models {
    
    public class DiMap {
        public virtual int DocumentAddress { get; set; }
        public virtual string Description { get; set; }
        public virtual int? Index { get; set; }
        public virtual bool Event { get; set; }
        public virtual int Level { get; set; }
        public virtual bool? Disable { get; set; }
        public virtual string Source { get; set; }
        public virtual string Name { get; set; }

        public virtual ushort Value { get; set; } = 0;
        //public virtual int EventGroupId { get; set; }
        public virtual IList<DiFlag> Flags { get; set; }
    }

    public class DiMapValue
    {
        public DiMap MapInfo { get; private set; }
        public ushort usValue { get; private set; }
       
    }
}
