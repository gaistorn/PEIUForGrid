using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataModel {

    public class AiMap {
        public virtual int DocumentAddress { get; set; }
        public virtual string Unit { get; set; }
        public virtual float Scale { get; set; }
        public virtual string Description { get; set; }
        public virtual string Name { get; set; }
        public virtual ushort Address { get; set; }
        public virtual int Type { get; set; }

        [JsonIgnore]
        public virtual DataType DataType {get;set;}
        public virtual short? Disable { get; set; }
        public virtual int? GroupId { get; set; }

        public virtual object ConvertValue(dynamic x)
        {
            if (Type == 1 || Scale == 0 || Scale == 1)
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
