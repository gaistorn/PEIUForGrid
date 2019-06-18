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

        public virtual object ConvertValue(object x)
        {
            if (Type == 1 || Scale == 0 || Scale == 1)
                return x;
            if (x is float)
                return (float)x * Scale;
            else
            {
                Int64 x64 = (Int64)x;
                return x64 * Scale;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
